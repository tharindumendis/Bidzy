using Bidzy.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Bidzy.Domain.Enties
{
    public class Auction
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        [Required]
        public Product Product { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal MinimumBid { get; set; }
        public AuctionStatus Status { get; set; } // Scheduled, Active, Ended, Cancelled
        public Guid? WinnerId { get; set; }
        public User? Winner { get; set; }

    }
}
