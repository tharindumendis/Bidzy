using Bidzy.Domain.Enum;

namespace Bidzy.Domain.Enties
{
    public class Auction
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal MinimumBid { get; set; }
        public AuctionStatus Status { get; set; } // Scheduled, Active, Ended, Cancelled
        public Guid? WinnerId { get; set; }
        public User Winner { get; set; }

    }
}
