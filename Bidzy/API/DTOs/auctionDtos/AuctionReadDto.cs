namespace Bidzy.API.DTOs.auctionDtos
{
    public class AuctionReadDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductTitle { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal MinimumBid { get; set; }
        public string Status { get; set; }
        public Guid? WinnerId { get; set; }
        public string? WinnerName { get; set; }
    }
}
