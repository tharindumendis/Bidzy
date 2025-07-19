namespace Bidzy.API.Dto
{
    public class BidAddDto
    {
        public Guid AuctionId { get; set; }
        public Guid BidderId { get; set; }
        public decimal Amount { get; set; }
    }
}
