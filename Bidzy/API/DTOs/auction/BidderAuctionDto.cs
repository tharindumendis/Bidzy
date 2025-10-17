using Bidzy.API.DTOs.bid;

namespace Bidzy.API.DTOs.auction
{
    public class BidderAuctionDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductTitle { get; set; }
        public string Description { get; set; }
        public Guid SellerId { get; set; }
        public string SellerName { get; set; }
        public string ImageUrl { get; set; }
        public string[] tags { get; set; }
        public string Category { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal MinimumBid { get; set; }
        public decimal? MaxBid { get; set; }
        public string Status { get; set; }
        public Guid? WinningBidId { get; set; }
        public decimal? WinAmount { get; set; }
        public Guid? WinnerId { get; set; }
        public List<BidReadDto> Bids { get; set; } = new List<BidReadDto>();
    }
}
