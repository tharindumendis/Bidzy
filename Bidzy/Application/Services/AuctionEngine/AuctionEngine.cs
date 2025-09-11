using Bidzy.API.DTOs;
using Bidzy.API.DTOs.auctionDtos;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services.NotificationEngine;
using Bidzy.Application.Services.Scheduler;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;

namespace Bidzy.Application.Services.AuctionEngine
{
    public class AuctionEngine : IAuctionEngine
    {
        private readonly IAuctionRepository _auctionRepo;
        private readonly IBidRepository _bidRepository;
        private readonly INotificationSchedulerService _scheduler;
        private readonly IJobScheduler _jobScheduler;
        private readonly ILiveAuctionCountService _liveAuctionCountService;
        private readonly INotificationService _notificationService;

        public AuctionEngine(
            IAuctionRepository auctionRepo,
            INotificationSchedulerService scheduler,
            INotificationService notificationService,
            IJobScheduler jobScheduler,
            IBidRepository bidRepository,
            ILiveAuctionCountService liveAuctionCountService)
        {
            _auctionRepo = auctionRepo;
            _scheduler = scheduler;
            _jobScheduler = jobScheduler;
            _notificationService = notificationService;
            _bidRepository = bidRepository;
            _liveAuctionCountService = liveAuctionCountService;
        }
        public async Task<AuctionReadDto> CreateAuctionAsync(AuctionAddDto dto)
        {
            var auction = dto.ToEntity();
            var saved = await _auctionRepo.AddAuctionAsync(auction);

            if (saved != null)
            {
                var delay = saved.StartTime - DateTime.UtcNow;
                if (delay.TotalSeconds > 0)
                {
                    _jobScheduler.Schedule<IAuctionEngine>(Services => Services.StartAuctionAsync(saved.Id), delay);
                    await _liveAuctionCountService.AddScheduledCount(1);
                }
                else
                {
                    StartAuctionAsync(saved.Id).Wait();
                }
            }

            return saved.ToReadDto();
        }

        public async Task StartAuctionAsync(Guid auctionId)
        {
            var auction = await _auctionRepo.GetAuctionDetailsByAuctionIdAsync(auctionId);
            auction.Status = AuctionStatus.Active;
            await _auctionRepo.UpdateAuctionAsync(auction);
            await _liveAuctionCountService.RemoveScheduledCount(1);
            await _liveAuctionCountService.AddOngoingCount(1);
            await _notificationService.NotifyAuctionStartedAsync(auction);
            var delay = auction.EndTime - DateTime.UtcNow;
            if (delay.TotalSeconds > 0)
            {
                _jobScheduler.Schedule<IAuctionEngine>(Service => Service.EndAuctionAsync(auction.Id), delay);
            }
            else
            {
                await EndAuctionAsync(auction.Id);
            }
        }

        public async Task EndAuctionAsync(Guid auctionId)
        {
            var auction = await _auctionRepo.GetAuctionDetailsByAuctionIdAsync(auctionId);
            
            Bid winBid = await DetermineWinner(auction);
            if(winBid == null)
            {
                CancelAuctionAsync(auctionId).Wait();
                return;
            }
            auction.WinningBidId = winBid.Id;
            auction.Status = AuctionStatus.Ended;
            auction = await _auctionRepo.UpdateAuctionAsync(auction);
            await _liveAuctionCountService.RemoveOngoingCount(1);
            await _notificationService.NotifyAuctionEndedAsync(auction, winBid);
        }

        public async Task CancelAuctionAsync(Guid auctionId)
        {

            var auction = await _auctionRepo.GetAuctionByIdAsync(auctionId);

            if(auction?.Status == AuctionStatus.Active)
            {
                await _liveAuctionCountService.RemoveOngoingCount(1);
            }
            else if (auction?.Status == AuctionStatus.Scheduled)
            {
                await _liveAuctionCountService.RemoveScheduledCount(1);
            }




            auction.Status = AuctionStatus.Cancelled;
            await _auctionRepo.UpdateAuctionAsync(auction);
            await _notificationService.NotifyAuctionCancelledAsync(auction);
        }
        private async Task<Bid?> DetermineWinner(Auction auction)
        {
            List<Bid> AllBids = await _bidRepository.GetBiddersByAuctionIdAsync(auction.Id);
            var validBids = AllBids
                .Where(bid => bid.Timestamp <= auction.EndTime)
                .OrderByDescending(bid => bid.Amount)      // Highest amount first
                .ThenBy(bid => bid.Timestamp)              // If tie, earliest bid wins
                .ToList();


            return validBids.FirstOrDefault();
        }

       
    }
}
