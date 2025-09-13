using System.Collections.Concurrent;
using System.Security.Claims;
using Bidzy.API.DTOs;
using Bidzy.API.DTOs.favoriteAuctionsDtos;
using Bidzy.Application.DTOs;
using Bidzy.Application.Repository;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services;
using Bidzy.Application.Services.Auth;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Bidzy.API.Hubs
{
    [Authorize]
    public class AuctionHub(IUserAuctionFavoriteRepository favoriteRepository, IAuthService authService, ILiveAuctionCountService liveCountService, INotificationRepository notificationRepository) : Hub
    {
        private readonly IUserAuctionFavoriteRepository _favoriteRepository = favoriteRepository;
        private readonly ILiveAuctionCountService _liveCountService = liveCountService;
        private readonly INotificationRepository _notificationRepository = notificationRepository;
        private readonly IAuthService _authService = authService;

        public static ConcurrentDictionary<string, HashSet<string>> GroupConnections = new();
        public static List<string> RoomIds = [];

        public async Task JoinAuctionGroup(HubSubscribeData payload)
        {
            

            try {
                if(payload.GroupIds == null) {  return; }

                foreach (string gId in payload.GroupIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, gId);
                    if (payload.UserId != null){
                        await AddFavorite(gId, payload.UserId);
                    }
                }
            }
            catch
            {
                return;
            }
        }



        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"User connected: {userId}");
           
            await base.OnConnectedAsync();
        }
        public async Task LeaveAuctionGroup(HubSubscribeData payload)
        {
            Console.WriteLine("leaveAuction"+payload.UserId);
            if (payload.GroupIds == null) { return; }
  
            foreach (string gId in payload.GroupIds)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId,gId);
                if (payload.UserId != null)
                {
                    await RemoveFavorite(gId, payload.UserId);
                }
            }
        }
        public async Task JoinAuctionRoom(JoinAuctionRoom payload)
        {
            ClaimsPrincipal principal = _authService.ValidateToken(payload.Token);
            try
            {
                // ADD Auction Room by "R" with AuctionID
                await Groups.AddToGroupAsync(Context.ConnectionId, "R"+payload.AuctionId);
   
            }
            catch
            {
                return;
            }
            await Clients.Group("R" + payload.AuctionId).SendAsync("ReceiveBidUpdate", "this is  new bid message");
        }
        public async Task LeaveAuctionRoom(JoinAuctionRoom payload)
        {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "R"+payload.AuctionId);
            //await Clients.Group("R" + payload.AuctionId).SendAsync("OnlineUpdate", groupCount);

        }
        public async Task SendBidUpdate(string auctionId, string message)
        {
            await Clients.Group(auctionId).SendAsync("ReceiveBidUpdate", message);
        }
        public async Task SendActionUpdate(string auctionId, string message)
        {
            await Clients.Group(auctionId).SendAsync("ReceiveAuctionUpdate", message);
        }



        // Notifications

        private static readonly ConcurrentDictionary<string, NotificationSubscribeDto> Connections = new();

        public async Task SubscribeToNotifications(NotificationSubscribeDto payload)
        {
            Connections[Context.ConnectionId] = payload;

            // Add user to group (e.g., auction ID)
            await _liveCountService.AddConnection(Context.ConnectionId, payload);


            await Groups.AddToGroupAsync(Context.ConnectionId, payload.UserId);
            await Groups.AddToGroupAsync(Context.ConnectionId, "App");
            await _liveCountService.BroadcastLiveCountAsync();
            // Notify others in the group 
            // this is temp
            //Notification newNotification = new()
            //{
            //    Id = Guid.NewGuid(),
            //    UserId = Guid.NewGuid(),
            //    Message = $"User {payload.UserId} connected to group {payload.GroupId}",
            //    Type = Domain.Enum.NotificationType.SYSTEM,
            //    IsSeen = false,
            //    Timestamp = DateTime.UtcNow
            //};
            //await Clients.Group("App").SendAsync("UserSubscribed", newNotification);

        }
        public async Task MarkNotificationAsSeen(NotificationSeenDto notificationSeenDto)
        {
            if (notificationSeenDto.NotificationId == null) return;

            Guid notificationId = Guid.Parse(notificationSeenDto.NotificationId);
            Guid userId = Guid.Parse(notificationSeenDto.UserId);

            await _notificationRepository.MarkAsSeenAsync(notificationId,userId);
        }
        public async Task MarkAllAsSeen(NotificationSeenDto notificationSeenDto)
        {
            Guid userId = Guid.Parse(notificationSeenDto.UserId);
            await _notificationRepository.MarkAllAsSeenByUserIdAsync(userId);
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Connections.TryRemove(Context.ConnectionId, out var user))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, user.GroupId);
                await _liveCountService.RemoveConnection(Context.ConnectionId);
                // this is temp for dev
                //Notification newNotification = new()
                //{
                //    Id = Guid.NewGuid(),
                //    UserId = Guid.NewGuid(),
                //    Message = $"User {user.UserId} connected to group {user.GroupId}",
                //    Type = Domain.Enum.NotificationType.SYSTEM,
                //    IsSeen = false,
                //    Timestamp = DateTime.UtcNow
                //};
                //await Clients.Group("App").SendAsync("UserUnsubscribed", newNotification);
            }
            Console.WriteLine($"Conn {Context.ConnectionId} disconnected. Reason: {exception?.Message}");

            await base.OnDisconnectedAsync(exception);
        }

























        private async Task AddFavorite (string auctionId, string userId)
        {
            if (Guid.TryParse(auctionId, out Guid aId))
            {
                if (Guid.TryParse(userId, out Guid uId))
                {
                    try
                    {
                        var favEntity = new UserAuctionFavorite
                        {
                            auctionId = aId,
                            userId = uId
                        };
                        await _favoriteRepository.AddAsync(favEntity);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
            return;
        }
        private async Task RemoveFavorite(string auctionId, string userId)
        {
            if (Guid.TryParse(auctionId, out Guid aId))
            {
                if (Guid.TryParse(userId, out Guid uId))
                {
                    try
                    {
                        await _favoriteRepository.RemoveAsync(uId, aId);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
            return;
        }
    }
}
