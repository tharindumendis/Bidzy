using System.ComponentModel.DataAnnotations;
using Bidzy.API.DTOs;
using Bidzy.API.DTOs.auctionDtos;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services.SignalR;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;

namespace Bidzy.Application.Services.AuctionEngine
{
    public class AuctionEngine : IAuctionEngine
    {
        private readonly IAuctionRepository _auctionRepo;
        private readonly INotificationSchedulerService _scheduler;
        private readonly ISignalRNotifier _notifier;
        private readonly IJobScheduler _jobScheduler;
        private readonly IEmailJobService _emailJobService;

        public AuctionEngine(
            IAuctionRepository auctionRepo,
            INotificationSchedulerService scheduler,
            IEmailJobService emailJobService,
            ISignalRNotifier notifier,
            IJobScheduler jobScheduler)
        {
            _auctionRepo = auctionRepo;
            _scheduler = scheduler;
            _notifier = notifier;
            _jobScheduler = jobScheduler;
            _emailJobService = emailJobService;
        }
        public async Task<Auction> CreateAuctionAsync(AuctionAddDto dto)
        {
            var auction = dto.ToEntity();
            var saved = await _auctionRepo.AddAuctionAsync(auction);

            if (saved != null)
            {
                var delay = saved.StartTime - DateTime.UtcNow;
                if (delay.TotalSeconds > 0)
                {
                    _jobScheduler.Schedule<IAuctionEngine>(Services => Services.StartAuctionAsync(saved.Id), delay);
                }
                else
                {
                    StartAuctionAsync(saved.Id).Wait();
                }
            }

            return saved;
        }

        public async Task StartAuctionAsync(Guid auctionId)
        {
            var auction = await _auctionRepo.GetAuctionByIdAsync(auctionId);
            auction.Status = AuctionStatus.Active;
            await _auctionRepo.UpdateAuctionAsync(auction);
            await _notifier.BroadcastAuctionStarted(auction);
            // TODO featch faverite bidders and send mail
            List<string> emailAddresses = null;
            await _emailJobService.SendAuctionStartedEmailsAsync(auction, emailAddresses);
            // TODO send favorite bidder Emails
            var delay = auction.StartTime - DateTime.UtcNow;
            if (delay.TotalSeconds > 0)
            {
                _jobScheduler.Schedule<IAuctionEngine>(Service => Service.EndAuctionAsync(auction.Id), delay);
            }
            else
            {
                EndAuctionAsync(auction.Id);
            }
        }

        public async Task EndAuctionAsync(Guid auctionId)
        {
            var auction = await _auctionRepo.GetAuctionByIdAsync(auctionId);
            auction.WinnerId = DetermineWinner(auction);
            if(auction.WinnerId == null)
            {
                CancelAuctionAsync(auctionId).Wait();
                return;
            }
            auction.Status = AuctionStatus.Ended;
            auction = await _auctionRepo.UpdateAuctionAsync(auction);

            await _notifier.BroadcastAuctionEnded(auction);
            // TODO : fetch winner Bid and set belove method
            await _emailJobService.SendAuctionEndedEmails(auction, new Bid());
        }

        public async Task CancelAuctionAsync(Guid auctionId)
        {
            var auction = await _auctionRepo.GetAuctionByIdAsync(auctionId);
            auction.Status = AuctionStatus.Cancelled;
            await _auctionRepo.UpdateAuctionAsync(auction);

            await _notifier.BroadcastAuctionCancelled(auction);
            //TODO: Notify bidders and sellers about cancellation
            await _emailJobService.SendAuctionCancelledEmail(auction);
        }

        private Guid? DetermineWinner(Auction auction)
        {
            

            // TODO: Implement bid evaluation logic
            return null;
        }
    }
}
