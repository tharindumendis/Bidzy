using Bidzy.API.DTOs.NotificationDtos;
using Bidzy.Domain.Enties;

namespace Bidzy.Application.Services.SignalR
{
    public interface ISignalRNotifier
    {
        Task BroadcastAuctionStarted(Auction auction);
        Task BroadcastAuctionEnded(Auction auction);
        Task BroadcastNewBid(Bid bid);
        Task BroadcastAuctionCancelled(Auction auction);
        Task BroadcastNotification(string groupId, NotificationDto notification);
        Task SendNotificationToUser(Notification notification);
        void SendNotificationToUsers(List<Notification> notifications);
    }
}
