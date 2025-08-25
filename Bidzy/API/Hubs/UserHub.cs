using System.Collections.Concurrent;
using Bidzy.Application.DTOs;
using Bidzy.Application.Repository;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Bidzy.API.Hubs
{
    public class UserHub : Hub
    {
        private readonly ILiveAuctionCountService _liveCountService;
        public UserHub(ILiveAuctionCountService liveCountService)
        {
            _liveCountService = liveCountService;
        }
        // Track active connections
        private static readonly ConcurrentDictionary<string, NotificationSubscribeDto> Connections = new();

        public async Task SubscribeToNotifications(NotificationSubscribeDto payload)
        {
            Connections[Context.ConnectionId] = payload;

            // Add user to group (e.g., auction ID)
            await _liveCountService.AddConnection(Context.ConnectionId, payload);

            await Groups.AddToGroupAsync(Context.ConnectionId, payload.GroupId);
            await Groups.AddToGroupAsync(Context.ConnectionId, "LiveCount");
            await _liveCountService.BroadcastLiveCountAsync();
            // Notify others in the group 
            // this is temp
            await Clients.Group(payload.GroupId).SendAsync("UserSubscribed", payload);

        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Connections.TryRemove(Context.ConnectionId, out var user))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, user.GroupId);
                await _liveCountService.RemoveConnection(Context.ConnectionId);
                // this is temp for dev
                await Clients.Group(user.GroupId).SendAsync("UserUnsubscribed", user);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
