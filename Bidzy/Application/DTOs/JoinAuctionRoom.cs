namespace Bidzy.Application.DTOs
{
    public class JoinAuctionRoom
    {
        public required string AuctionId { get; set; }
        public required string UserId { get; set; }
        public required string Token { get; set; }
    }
}
