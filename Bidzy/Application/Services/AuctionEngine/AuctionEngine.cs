using System.ComponentModel.DataAnnotations;
using Bidzy.API.DTOs;
using Bidzy.API.DTOs.auctionDtos;
using Bidzy.API.Hubs;
using Bidzy.Application.Repository;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services.SignalR;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;
using Microsoft.AspNetCore.SignalR;

namespace Bidzy.Application.Services.AuctionEngine
{
    public class AuctionEngine : IAuctionEngine
    {
        private readonly IAuctionRepository _auctionRepo;
        private readonly IBidRepository _bidRepository;
        private readonly INotificationSchedulerService _scheduler;
        private readonly ISignalRNotifier _notifier;
        private readonly IJobScheduler _jobScheduler;
        private readonly IEmailJobService _emailJobService;
        private readonly ILiveAuctionCountService _liveAuctionCountService;
        private readonly INotificationRepository _notificationRepository;



        public AuctionEngine(
            IAuctionRepository auctionRepo,
            INotificationSchedulerService scheduler,
            IEmailJobService emailJobService,
            ISignalRNotifier notifier,
            IJobScheduler jobScheduler,
            IBidRepository bidRepository,
            ILiveAuctionCountService liveAuctionCountService,
            INotificationRepository notificationRepository)
        {
            _auctionRepo = auctionRepo;
            _scheduler = scheduler;
            _notifier = notifier;
            _jobScheduler = jobScheduler;
            _emailJobService = emailJobService;
            _bidRepository = bidRepository;
            _liveAuctionCountService = liveAuctionCountService;
            _notificationRepository = notificationRepository;
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
            var auction = await _auctionRepo.GetAuctionByIdAsync(auctionId);
            auction.Status = AuctionStatus.Active;
            await _auctionRepo.UpdateAuctionAsync(auction);
            await _liveAuctionCountService.RemoveScheduledCount(1);
            await _liveAuctionCountService.AddOngoingCount(1);
            await _notifier.BroadcastAuctionStarted(auction);
            await _notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
            {
                Id = Guid.NewGuid(),
                UserId = auction.Product.SellerId,
                Message = $"Your auction '{auction.Product.Title}' has started.",
                Type = NotificationType.Auction,
                SentAt = DateTime.UtcNow,
                IsSeen = false
            });
            // TODO featch faverite bidders and send mail
            List<string> emailAddresses = null;
            await _emailJobService.SendAuctionStartedEmailsAsync(auction, emailAddresses);
            // TODO send favorite bidder Emails
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
            var auction = await _auctionRepo.GetAuctionByIdAsync(auctionId);
            
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
            await _notifier.BroadcastAuctionEnded(auction);
            await _emailJobService.SendAuctionEndedEmails(auction, winBid);
            await _notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
            {
                Id = Guid.NewGuid(),
                UserId = auction.Product.SellerId,
                Message = $"Your auction '{auction.Product.Title}' has ended. Winning bid: {winBid.Amount:C} by User {winBid.BidderId}.",
                Type = NotificationType.Auction,
                SentAt = DateTime.UtcNow,
                IsSeen = false
            });
            await _notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
            {
                Id = Guid.NewGuid(),
                UserId = winBid.BidderId,
                Message = $"Congratulations! You won the auction '{auction.Product.Title}' with a bid of {winBid.Amount:C}.",
                Type = NotificationType.Bid,
                SentAt = DateTime.UtcNow,
                IsSeen = false
            });
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
           
            await _notifier.BroadcastAuctionCancelled(auction);
            //TODO: Notify bidders and sellers about cancellation
            await _emailJobService.SendAuctionCancelledEmail(auction);

            await _notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
            {
                Id = Guid.NewGuid(),
                UserId = auction.Product.SellerId,
                Message = $"Your auction '{auction.Product.Title}' has been cancelled.",
                Type = NotificationType.Auction,
                SentAt = DateTime.UtcNow,
                IsSeen = false
            });
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
