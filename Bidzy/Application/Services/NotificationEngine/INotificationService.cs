using Bidzy.Domain.Enties;

namespace Bidzy.Application.Services.NotificationEngine
{
    public interface INotificationService
    {
        Task NotifyAuctionStartedAsync(Auction auction);
        Task NotifyAuctionEndedAsync(Auction auction, Bid winningBid);
        Task NotifyAuctionCancelledAsync(Auction auction);
        //Task NotifyNewBidPlacedAsync(int auctionId, int bidId);
    }
}
