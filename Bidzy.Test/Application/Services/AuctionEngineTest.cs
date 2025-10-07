using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bidzy.API.DTOs.auctionDtos;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services;
using Bidzy.Application.Services.AuctionEngine;
using Bidzy.Application.Services.NotificationEngine;
using Bidzy.Application.Services.Scheduler;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;
using FakeItEasy;
using Xunit;

namespace Bidzy.Test.Application.Services
{
    public class AuctionEngineTest
    {
        private readonly IAuctionRepository _auctionRepo;
        private readonly IBidRepository _bidRepository;
        private readonly INotificationSchedulerService _scheduler;
        private readonly IJobScheduler _jobScheduler;
        private readonly ILiveAuctionCountService _liveAuctionCountService;
        private readonly INotificationService _notificationService;

        public AuctionEngineTest()
        {
            _auctionRepo = A.Fake<IAuctionRepository>();
            _bidRepository = A.Fake<IBidRepository>();
            _scheduler = A.Fake<INotificationSchedulerService>();
            _jobScheduler = A.Fake<IJobScheduler>();
            _liveAuctionCountService = A.Fake<ILiveAuctionCountService>();
            _notificationService = A.Fake<INotificationService>();
        }

        [Fact]
        public async Task CreateAuctionAsync_ShouldScheduleJob_WhenStartTimeInFuture()
        {
            // Arrange
            Guid newProductId = Guid.NewGuid();
            Guid newAuctionId = Guid.NewGuid();

            var auctionAddDto = new AuctionAddDto
            {
                ProductId = newProductId,
                Category = AuctionCategories.Photography,
                StartTime = DateTime.UtcNow.AddMinutes(10),
                EndTime = DateTime.UtcNow.AddHours(1),
                MinimumBid = 100
            };

            var savedAuction = new Auction
            {
                Id = newAuctionId,
                ProductId = newProductId,
                Product = new Product { Id = newProductId },
                Category = AuctionCategories.Photography,
                StartTime = auctionAddDto.StartTime,
                EndTime = auctionAddDto.EndTime,
                MinimumBid = auctionAddDto.MinimumBid,
                Status = AuctionStatus.Scheduled
            };

            A.CallTo(() => _auctionRepo.AddAuctionAsync(A<Auction>.Ignored))
                .Returns(Task.FromResult(savedAuction));

            A.CallTo(() => _liveAuctionCountService.AddScheduledCount(1))
                .Returns(Task.CompletedTask);

            var auctionEngine = new AuctionEngine(
                _auctionRepo, _scheduler, _notificationService,
                _jobScheduler, _bidRepository, _liveAuctionCountService);

            // Act
            var result = await auctionEngine.CreateAuctionAsync(auctionAddDto);

            // Assert
            A.CallTo(() => _jobScheduler.Schedule<IAuctionEngine>(
                A<Expression<Action<IAuctionEngine>>>._,
                A<TimeSpan>.That.Matches(t => t.TotalSeconds > 0)))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _liveAuctionCountService.AddScheduledCount(1))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task CreateAuctionAsync_ShouldThrowException_WhenStartTimeInPast()
        {
            var auctionAddDto = new AuctionAddDto
            {
                ProductId = Guid.NewGuid(),
                Category = AuctionCategories.Photography,
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                EndTime = DateTime.UtcNow.AddHours(1),
                MinimumBid = 100
            };

            var auctionEngine = new AuctionEngine(
                _auctionRepo, _scheduler, _notificationService,
                _jobScheduler, _bidRepository, _liveAuctionCountService);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                auctionEngine.CreateAuctionAsync(auctionAddDto));

            Assert.Equal("Start time must be in the future.", ex.Message);
        }

        [Fact]
        public async Task CreateAuctionAsync_ShouldThrowException_WhenEndTimeBeforeStartTime()
        {
            var auctionAddDto = new AuctionAddDto
            {
                ProductId = Guid.NewGuid(),
                Category = AuctionCategories.Photography,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddMinutes(30),
                MinimumBid = 100
            };

            var auctionEngine = new AuctionEngine(
                _auctionRepo, _scheduler, _notificationService,
                _jobScheduler, _bidRepository, _liveAuctionCountService);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                auctionEngine.CreateAuctionAsync(auctionAddDto));

            Assert.Equal("End time must be after start time.", ex.Message);
        }

        [Fact]
        public async Task CreateAuctionAsync_ShouldThrowException_WhenMinimumBidIsZero()
        {
            var auctionAddDto = new AuctionAddDto
            {
                ProductId = Guid.NewGuid(),
                Category = AuctionCategories.Photography,
                StartTime = DateTime.UtcNow.AddMinutes(10),
                EndTime = DateTime.UtcNow.AddHours(1),
                MinimumBid = 0
            };

            var auctionEngine = new AuctionEngine(
                _auctionRepo, _scheduler, _notificationService,
                _jobScheduler, _bidRepository, _liveAuctionCountService);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                auctionEngine.CreateAuctionAsync(auctionAddDto));

            Assert.Equal("Minimum bid must be greater than zero.", ex.Message);
        }

        [Fact]
        public async Task CreateAuctionAsync_ShouldThrowException_WhenCategoryIsInvalid()
        {
            var auctionAddDto = new AuctionAddDto
            {
                ProductId = Guid.NewGuid(),
                Category = (AuctionCategories)999, // Invalid enum
                StartTime = DateTime.UtcNow.AddMinutes(10),
                EndTime = DateTime.UtcNow.AddHours(1),
                MinimumBid = 100
            };

            var auctionEngine = new AuctionEngine(
                _auctionRepo, _scheduler, _notificationService,
                _jobScheduler, _bidRepository, _liveAuctionCountService);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                auctionEngine.CreateAuctionAsync(auctionAddDto));

            Assert.Equal("Invalid auction category.", ex.Message);
        }

        [Fact]
        public async Task CreateAuctionAsync_ShouldThrowWrappedException_WhenRepositoryFails()
        {
            var auctionAddDto = new AuctionAddDto
            {
                ProductId = Guid.NewGuid(),
                Category = AuctionCategories.Photography,
                StartTime = DateTime.UtcNow.AddMinutes(10),
                EndTime = DateTime.UtcNow.AddHours(1),
                MinimumBid = 100
            };

            A.CallTo(() => _auctionRepo.AddAuctionAsync(A<Auction>.Ignored))
                .Throws(new Exception("DB failure"));

            var auctionEngine = new AuctionEngine(
                _auctionRepo, _scheduler, _notificationService,
                _jobScheduler, _bidRepository, _liveAuctionCountService);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                auctionEngine.CreateAuctionAsync(auctionAddDto));

            Assert.Equal("Error Saving Auction.", ex.Message);
            Assert.Equal("DB failure", ex.InnerException?.Message);
        }
    }
}