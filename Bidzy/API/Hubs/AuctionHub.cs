using Bidzy.Application.DTOs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Bidzy.API.Hubs
{
    public class AuctionHub : Hub
    {
        public async Task JoinAuctionGroup(HubSubscribeData payload)
        {
            try {
                if(payload.GroupIds == null) {  return; }

                foreach (string item in payload.GroupIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, item);
                }
            }
            catch
            {
                return;
            }
        }
        public async Task LeaveAuctionGroup(HubSubscribeData payload)
        {
            Console.WriteLine("leaveAuction"+payload.UserId);
            if (payload.GroupIds == null) { return; }
  
            foreach (string groupId in payload.GroupIds)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId,groupId);
                
            }
        }
        public async Task SendBidUpdate(string auctionId, string message)
        {
            await Clients.Group(auctionId).SendAsync("ReceiveBidUpdate", message);
        }
        public async Task SendActionUpdate(string auctionId, string message)
        {
            await Clients.Group(auctionId).SendAsync("ReceiveAuctionUpdate", message);
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Conn {Context.ConnectionId} disconnected. Reason: {exception?.Message}");

            await base.OnDisconnectedAsync(exception);
        }

    }
}
