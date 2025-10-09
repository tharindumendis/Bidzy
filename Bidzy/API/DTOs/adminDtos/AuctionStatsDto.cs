namespace Bidzy.API.DTOs.adminDtos
{
    public class AuctionStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Ended { get; set; }
        public int Canceled { get; set; }
        public decimal Revenue { get; set; }
    }
}
