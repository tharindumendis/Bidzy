using Bidzy.API.DTOs.NotificationDtos;
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
            // I put this first for send the webSocket message for Quick UI update
            await signalRNotifier.BroadcastAuctionStarted(auction);
       
            List<UserAuctionFavorite>? likedUsers = auction.LikedByUsers?.ToList();
            List<string>? likedUsersEmailAddress = likedUsers?.Select(user => user.user.Email).ToList();
            List<Notification> NotificationsList = [];

            NotificationsList.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = auction.Product.SellerId,
                Message = $"Your auction for {auction.Product.Title} has started.",
                Type = NotificationType.AUCTIONSTART,
                Link = auction.Id.ToString(),
                IsSeen = false
            });
            likedUsers?.ForEach(fav =>
                {
                    NotificationsList.Add(new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = fav.userId,
                        Message = $"An auction for {auction.Product.Title} you favorited has started.",
                        Type = NotificationType.AUCTIONSTART,
                        Link = auction.Id.ToString(),
                        IsSeen = false
                    });
                });
            signalRNotifier.SendNotificationToUsers(NotificationsList);
            await notificationRepository.AddNotificationsAsync(NotificationsList);
            if (!(likedUsersEmailAddress == null || likedUsersEmailAddress.Count == 0))
            {
                await emailJobService.SendAuctionStartedEmailsAsync(auction, likedUsersEmailAddress);      
            }
        }

        public async Task NotifyAuctionEndedAsync(Auction auction, Bid winningBid)
        {
            await signalRNotifier.BroadcastAuctionEnded(auction);


            List<Notification> NotificationsList = [];
            NotificationsList.Add( new Notification
            {
                Id = Guid.NewGuid(),
                UserId = auction.Product.SellerId,
                Message = $"Your auction for {auction.Product.Title} has ended. Winning bid: {winningBid.Amount:C}.",
                Type = NotificationType.AUCTIONEND,
                Link = auction.Id.ToString(),
                IsSeen = false
            });
            NotificationsList.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = winningBid.BidderId,
                Message = $"Congratulations! You won the auction for {auction.Product.Title} with a bid of {winningBid.Amount:C}.",
                Type = NotificationType.AUCTIONEND,
                IsSeen = false
            });
            // send notification to all users who participated in this auction but did not win
            auction.participations?.ToList().ForEach(participation =>
            {
                if (participation.userId != winningBid.BidderId)
                {
                    NotificationsList.Add(new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = participation.userId,
                        Message = $"The auction for {auction.Product.Title} has ended. The winning bid was {winningBid.Amount:C}. Better luck next time!",
                        Type = NotificationType.AUCTIONEND,
                        Link = auction.Id.ToString(),
                        IsSeen = false
                    });
                }
            });
            // send notification to all users who favorited this auction
            auction.LikedByUsers?.ToList().ForEach(fav =>
            {
                NotificationsList.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = fav.userId,
                    Message = $"The auction for {auction.Product.Title} you favorited has ended. The winning bid was {winningBid.Amount:C}.",
                    Type = NotificationType.AUCTIONEND,
                    Link = auction.Id.ToString(),
                    IsSeen = false
                });
            });
            signalRNotifier.SendNotificationToUsers(NotificationsList);
            await notificationRepository.AddNotificationsAsync(NotificationsList);
            await emailJobService.SendAuctionEndedEmails(auction, winningBid);
        }
        public async Task NotifyAuctionCancelledAsync(Auction targetAuction)
        {
            await signalRNotifier.BroadcastAuctionCancelled(targetAuction);


            List<Notification> NotificationsList = [];

            NotificationsList.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = targetAuction.Product.SellerId,
                Message = $"Your auction for {targetAuction.Product.Title} has been cancelled.",
                Type = NotificationType.AUCTIONCANCLLED,
                Link = targetAuction.Id.ToString(),
                IsSeen = false
            });
            // send notification to all users who biddes in this auction
            targetAuction.participations?.ToList().ForEach(participation =>
            {
                NotificationsList.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = participation.userId,
                    Message = $"The auction for {targetAuction.Product.Title} you participated in has been cancelled.",
                    Type = NotificationType.AUCTIONCANCLLED,
                    Link = targetAuction.Id.ToString(),
                    IsSeen = false
                });
            });
            // send notification to all users who favorited this auction
            targetAuction.LikedByUsers?.ToList().ForEach(fav =>
            {
                NotificationsList.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = fav.userId,
                    Message = $"The auction for {targetAuction.Product.Title} you favorited has been cancelled.",
                    Type = NotificationType.AUCTIONCANCLLED,
                    Link = targetAuction.Id.ToString(),
                    IsSeen = false
                });
            });
            signalRNotifier.SendNotificationToUsers(NotificationsList);
            await notificationRepository.AddNotificationsAsync(NotificationsList);
            await emailJobService.SendAuctionCancelledEmail(targetAuction);

        }
    }
}
