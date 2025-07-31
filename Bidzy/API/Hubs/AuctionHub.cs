using Microsoft.AspNetCore.SignalR;

namespace Bidzy.API.Hubs
{
    public class AuctionHub : Hub
    {
        public async Task JoinAuctionGroup(string auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, auctionId);
            Console.WriteLine("web socket test :" + auctionId);
        }

        public async Task LeaveAuctionGroup(string auctionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId);
        }
        public async Task SendBidUpdate(string auctionId, string message)
        {
            await Clients.Group(auctionId).SendAsync("ReceiveBidUpdate", message);
        }
        public async Task SendActionUpdate(string auctionId, string message)
        {
            await Clients.Group(auctionId).SendAsync("ReceiveAuctionUpdate", message);
        }
    }
}
