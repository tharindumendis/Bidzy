using Bidzy.API.DTOs;
using Bidzy.API.Hubs;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.SignalR;

namespace Bidzy.Application.Services.SignalR
{
    public class SignalRNotifier : ISignalRNotifier
    {
        private readonly IHubContext<AuctionHub> _hubContext;

        public SignalRNotifier(IHubContext<AuctionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BroadcastAuctionStarted(Auction auction)
        {
            await _hubContext.Clients.Group(auction.Id.ToString())
                .SendAsync("AuctionStarted", auction.ToReadDto());
        }

        public async Task BroadcastAuctionEnded(Auction auction)
        {
            await _hubContext.Clients.Group(auction.Id.ToString())
                .SendAsync("AuctionEnded", auction.ToReadDto());
        }

        public async Task BroadcastNewBid(Bid bid)
        {
            await _hubContext.Clients.Group(bid.AuctionId.ToString())
                .SendAsync("NewBidPlaced", bid.ToReadDto());
        }
        // TODO
        public Task BroadcastAuctionCancelled(Auction auction)
        {
            throw new NotImplementedException();
        }
    }
}
