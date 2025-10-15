namespace Bidzy.API.DTOs.Admin
{
    public class SiteAnalyticsDto
    {
        public UserStatsDto UserStats { get; set; }
        public ProductStatsDto ProductStats { get; set; }
        public AuctionStatsDto AuctionStats { get; set; }
    }
}
