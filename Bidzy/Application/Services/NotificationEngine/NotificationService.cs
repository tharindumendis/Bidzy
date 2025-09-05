using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services.SignalR;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;

namespace Bidzy.Application.Services.NotificationEngine
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository notificationRepository;
        private readonly ISignalRNotifier signalRNotifier;
        private readonly IEmailJobService emailJobService;

        public NotificationService(
            INotificationRepository notificationRepository,
            ISignalRNotifier signalRNotifier,
            IEmailJobService emailJobService
            )
        {
            this.notificationRepository = notificationRepository;
            this.signalRNotifier = signalRNotifier;
            this.emailJobService = emailJobService;
        }

        public async Task NotifyAuctionStartedAsync(Auction auction)
        {
            await notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
            {
                Id = Guid.NewGuid(),
                UserId = auction.Product.SellerId,
                Message = $"Your auction for {auction.Product.Title} has started.",
                Type = NotificationType.AUCTIONSTART,
                IsSeen = false
            });
            // send notification to all users who favorited this auction

            await signalRNotifier.BroadcastAuctionStarted(auction);
            List<string> emailAddresses = null;
            await emailJobService.SendAuctionStartedEmailsAsync(auction, emailAddresses);
        }

        public async Task NotifyAuctionEndedAsync(Auction auction, Bid winningBid)
        {
            await notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
            {
                Id = Guid.NewGuid(),
                UserId = auction.Product.SellerId,
                Message = $"Your auction for {auction.Product.Title} has ended. Winning bid: {winningBid.Amount:C}.",
                Type = NotificationType.AUCTIONEND,
                IsSeen = false
            });
            await notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
            {
                Id = Guid.NewGuid(),
                UserId = winningBid.BidderId,
                Message = $"Congratulations! You won the auction for {auction.Product.Title} with a bid of {winningBid.Amount:C}.",
                Type = NotificationType.AUCTIONEND,
                IsSeen = false
            });
            // send notification to all users who participated in this auction but did not win
            // send notification to all users who favorited this auction

            await signalRNotifier.BroadcastAuctionEnded(auction);
            await emailJobService.SendAuctionEndedEmails(auction, winningBid);
        }
        public async Task NotifyAuctionCancelledAsync(Auction auction)
        {
            await notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
            {
                Id = Guid.NewGuid(),
                UserId = auction.Product.SellerId,
                Message = $"Your auction for {auction.Product.Title} has been cancelled.",
                Type = NotificationType.AUCTIONCANCLLED,
                IsSeen = false
            });
            // send notification to all users who biddes in this auction
            // send notification to all users who favorited this auction

            await signalRNotifier.BroadcastAuctionCancelled(auction);
            await emailJobService.SendAuctionCancelledEmail(auction);
        }
    }
}
