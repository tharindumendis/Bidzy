using Bidzy.Domain.Enties;

namespace Bidzy.Application.Services.SignalR
{
    public interface ISignalRNotifier
    {
        Task BroadcastAuctionStarted(Auction auction);
        Task BroadcastAuctionEnded(Auction auction);
        Task BroadcastNewBid(Bid bid);

    }
}
