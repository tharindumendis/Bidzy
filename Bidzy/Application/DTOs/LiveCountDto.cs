namespace Bidzy.Application.DTOs
{
    public class LiveCountDto
    {
        public int? UserCount { get; set; } = 0;
        public int? ScheduledAuctionCount { get; set; } = 0;
        public int? OngoingAuctionCount { get; set; } = 0;

    }
}
