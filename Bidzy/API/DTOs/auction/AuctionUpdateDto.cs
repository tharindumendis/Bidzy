using Bidzy.Domain.Enum;

namespace Bidzy.API.DTOs.auction
{
    public class AuctionUpdateDto
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? MinimumBid { get; set; }
        public string? Status { get; set; } 
        public Guid? WinnerId { get; set; }
        public AuctionCategories? Category { get; set; }
    }
}
