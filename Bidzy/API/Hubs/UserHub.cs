using System.Collections.Concurrent;
using Bidzy.Application.DTOs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Bidzy.API.Hubs
{
    public class UserHub : Hub
    {
        // Track active connections
        private static readonly ConcurrentDictionary<string, NotificationSubscribeDto> Connections = new();

        // Called when a user subscribes to notifications
        public async Task SubscribeToNotifications(NotificationSubscribeDto payload)
        {
            // Store connection info
            Connections[Context.ConnectionId] = payload;

            // Add user to group (e.g., auction ID)
            await Groups.AddToGroupAsync(Context.ConnectionId, payload.GroupId);

            // Notify others in the group
            await Clients.Group(payload.GroupId).SendAsync("UserSubscribed", payload);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Connections.TryRemove(Context.ConnectionId, out var user))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, user.GroupId);
                await Clients.Group(user.GroupId).SendAsync("UserUnsubscribed", user);
            }

            await base.OnDisconnectedAsync(exception);
        }

        
    }

}
