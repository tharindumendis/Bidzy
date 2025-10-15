using Bidzy.Domain.Entities;

namespace Bidzy.Application.Services.NotificationEngine
{
    public interface INotificationService
    {
        Task NotifyAuctionStartedAsync(Auction auction);
        Task NotifyAuctionEndedAsync(Auction auction, Domain.Entities.Bid winningBid);
        Task NotifyAuctionCancelledAsync(Auction auction);
        Task NotifyPaymentFailedAsync(Payment payment, User buyer, Auction auction, string reason);
        Task NotifyPaymentRefundedAsync(Payment payment, User buyer, Auction auction);
        //Task NotifyNewBidPlacedAsync(int auctionId, int bidId);
    }
}
