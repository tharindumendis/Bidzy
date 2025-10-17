namespace Bidzy.API.DTOs.bid
{
    public class BidReadDto
    {
        public Guid Id { get; set; }
        public Guid AuctionId { get; set; }
        public string Title { get; set; }
        public Guid BidderId { get; set; }
        public string BidderName { get; set; }
        public string BidderImageUrl { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
