using Bidzy.API.DTOs.bid;
using Bidzy.Domain.Enum;

namespace Bidzy.API.DTOs.auction
{
    public class ShopAuctionDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductTitle { get; set; }
        public string[] tags { get; set; }
        public string? Category { get; set; }
        public string ImageUrl { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal MinimumBid { get; set; }
        public string Status { get; set; }
        public BidReadDto WinBid { get; set; }
        public List<BidReadDto> Bids { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int ParticipationCount { get; set; }

    }
}
