using Bidzy.API.DTOs.adminDtos;

namespace Bidzy.API.DTOs
{
    public class SiteAnalyticsDto
    {
        public UserStatsDto UserStats { get; set; }
        public ProductStatsDto ProductStats { get; set; }
        public AuctionStatsDto AuctionStats { get; set; }
    }
}
