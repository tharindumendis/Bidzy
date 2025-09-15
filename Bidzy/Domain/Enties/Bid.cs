namespace Bidzy.Domain.Enties
{
    public class Bid
    {
        public Guid Id { get; set; }
        public Guid AuctionId { get; set; }
        public Auction Auction { get; set; }
        public Guid BidderId { get; set; }
        public User Bidder { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRefunded { get; set; } = false;
    }
}
