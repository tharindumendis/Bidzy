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
            auction.LikedByUsers?.ToList().ForEach(async fav =>
                {
                    await notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = fav.userId,
                        Message = $"An auction for {auction.Product.Title} you favorited has started.",
                        Type = NotificationType.AUCTIONSTART,
                        IsSeen = false
                    });
                });

            await notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
            {
                Id = Guid.NewGuid(),
                UserId = auction.Product.SellerId,
                Message = $"Your auction for {auction.Product.Title} has started.",
                Type = NotificationType.AUCTIONSTART,
                IsSeen = false
            });
            

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
            auction.participations?.ToList().ForEach(async participation =>
            {
                if (participation.userId != winningBid.BidderId)
                {
                    await notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = participation.userId,
                        Message = $"The auction for {auction.Product.Title} has ended. The winning bid was {winningBid.Amount:C}. Better luck next time!",
                        Type = NotificationType.AUCTIONEND,
                        IsSeen = false
                    });
                }
            });
            // send notification to all users who favorited this auction
            auction.LikedByUsers?.ToList().ForEach(async fav =>
            {
                await notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = fav.userId,
                    Message = $"The auction for {auction.Product.Title} you favorited has ended. The winning bid was {winningBid.Amount:C}.",
                    Type = NotificationType.AUCTIONEND,
                    IsSeen = false
                });
            });

            await signalRNotifier.BroadcastAuctionEnded(auction);
            await emailJobService.SendAuctionEndedEmails(auction, winningBid);
        }
        public async Task NotifyAuctionCancelledAsync(Auction targetAuction)
        {
            await notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
            {
                Id = Guid.NewGuid(),
                UserId = targetAuction.Product.SellerId,
                Message = $"Your auction for {targetAuction.Product.Title} has been cancelled.",
                Type = NotificationType.AUCTIONCANCLLED,
                IsSeen = false
            });
            // send notification to all users who biddes in this auction
            targetAuction.participations?.ToList().ForEach(async participation =>
            {
                await notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = participation.userId,
                    Message = $"The auction for {targetAuction.Product.Title} you participated in has been cancelled.",
                    Type = NotificationType.AUCTIONCANCLLED,
                    IsSeen = false
                });
            });
            // send notification to all users who favorited this auction
            targetAuction.LikedByUsers?.ToList().ForEach(async fav =>
            {
                await notificationRepository.AddNotificationAsync(new Domain.Enties.Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = fav.userId,
                    Message = $"The auction for {targetAuction.Product.Title} you favorited has been cancelled.",
                    Type = NotificationType.AUCTIONCANCLLED,
                    IsSeen = false
                });
            });

            await signalRNotifier.BroadcastAuctionCancelled(targetAuction);
            await emailJobService.SendAuctionCancelledEmail(targetAuction);
        }
    }
}
