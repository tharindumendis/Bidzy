using Bidzy.API.DTOs.Admin;
using Bidzy.API.DTOs.auctionDtos;
using Bidzy.API.DTOs.bidDtos;
using Bidzy.API.DTOs.userDtos;

namespace Bidzy.Application.Services.Admin
{
    public interface IAdminDashboardHubService
    {
        Task BroadcastAnalyticsUpdate(SiteAnalyticsDto siteAnalyticsDto);
        Task BroadcastNewUser(UserReadDto userReadDto);
        Task BroadcastAuctionUpdate(AuctionReadDto auctionReadDto);
        Task BroadcastNewBid (BidReadDto bidReadDto);
        Task BroadcastLiveUsersUpdate();
    }
}
