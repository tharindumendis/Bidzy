using System.Collections.Concurrent;
using Bidzy.Application.DTOs;
using Bidzy.Application.Repository;
using Bidzy.Application.Services.LiveService;
using Bidzy.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Bidzy.API.Hubs
{
    public class UserHub(ILiveAuctionCountService liveCountService) : Hub
    {
        private readonly ILiveAuctionCountService _liveCountService = liveCountService;

        // Track active connections
        private static readonly ConcurrentDictionary<string, NotificationSubscribeDto> Connections = new();

        public async Task SubscribeToNotifications(NotificationSubscribeDto payload)
        {
            Connections[Context.ConnectionId] = payload;

            // Add user to group (e.g., auction ID)


            await Groups.AddToGroupAsync(Context.ConnectionId, payload.UserId);
            await Groups.AddToGroupAsync(Context.ConnectionId, "App");
            await _liveCountService.BroadcastLiveCountAsync();
            // Notify others in the group 
            // this is temp
            Notification newNotification = new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Message = $"User {payload.UserId} connected to group {payload.GroupId}",
                Type = Domain.Enum.NotificationType.SYSTEM,
                IsSeen = false,
                Timestamp = DateTime.UtcNow
            };
            await Clients.Group("App").SendAsync("UserSubscribed", newNotification);

        }

        public override async Task OnConnectedAsync()
        {
            await _liveCountService.AddConnection(Context.ConnectionId, null);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _liveCountService.RemoveConnection(Context.ConnectionId);

            if (Connections.TryRemove(Context.ConnectionId, out var user))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, user.GroupId);
                // this is temp for dev
                Notification newNotification = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Message = $"User {user.UserId} connected to group {user.GroupId}",
                    Type = Domain.Enum.NotificationType.SYSTEM,
                    IsSeen = false,
                    Timestamp = DateTime.UtcNow
                };
                await Clients.Group("App").SendAsync("UserUnsubscribed", user);
            }
            await base.OnDisconnectedAsync(exception);
        }
        
    }
}
