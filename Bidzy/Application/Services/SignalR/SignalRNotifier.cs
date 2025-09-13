using System.Text.RegularExpressions;
using Bidzy.API.DTOs;
using Bidzy.API.DTOs.NotificationDtos;
using Bidzy.API.Hubs;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.SignalR;

namespace Bidzy.Application.Services.SignalR
{
    public class SignalRNotifier(IHubContext<AuctionHub> hubContext, IHubContext<UserHub> guestHubContext) : ISignalRNotifier
    {
        private readonly IHubContext<AuctionHub> _hubContext = hubContext;
        private readonly IHubContext<UserHub> _guestHubContext = guestHubContext;

        public async Task BroadcastAuctionStarted(Auction auction)
        {
            await _guestHubContext.Clients.Group(auction.Id.ToString())
                .SendAsync("AuctionStarted", auction.ToReadDto());
        }

        public async Task BroadcastAuctionEnded(Auction auction)
        {
            await _guestHubContext.Clients.Group(auction.Id.ToString())
                .SendAsync("AuctionEnded", auction.ToReadDto());
        }

        public async Task BroadcastNewBid(Bid bid)
        {
            await _hubContext.Clients.Group("R" +bid.AuctionId.ToString())
                .SendAsync("ReceiveBidUpdate", bid.ToReadDto());
        }
        // TODO
        public async Task BroadcastAuctionCancelled(Auction auction)
        {
            await _guestHubContext.Clients.Group(auction.Id.ToString())
                .SendAsync("AuctionCancelled", auction.ToReadDto());
        }
        public async Task BroadcastNotification(string groupId, NotificationDto notification)
        {
            await _hubContext.Clients.Group(groupId)
                .SendAsync("ReceiveNotification", notification);
        }
        public async Task SendNotificationToUser(Notification notification)
        {
            NotificationDto notificationDto = new NotificationDto
            {
                Id = notification.Id,
                IsSeen = notification.IsSeen,
                Message = notification.Message,
                Link = notification.Link,
                Timestamp = notification.Timestamp,
                Type = notification.Type.ToString()
            };
            await _hubContext.Clients.Group(notification.UserId.ToString())
                .SendAsync("ReceiveNotification", notificationDto);
        }
        public void SendNotificationToUsers(List<Notification> notifications)
        {
            
            notifications.ForEach(async notification =>
            {
                await SendNotificationToUser(notification);
            });
        }
    }
}
