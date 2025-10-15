namespace Bidzy.Domain.Entities
{
    public class ViewHistory
    {
        public Guid Id { get; set; }
        public required User User { get; set; }
        public Guid UserId { get; set; }
        public required Auction Auction { get; set; }
        public Guid AuctionId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
