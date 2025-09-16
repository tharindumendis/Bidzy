namespace Bidzy.API.DTOs.auctionDtos
{
    public class AuctionReadDto
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
        public string Status { get; set; }
        public Guid? WinningBidId { get; set; }
        public decimal? WinAmount { get; set; }
        public string? WinningBid { get; set; }
    }
}
