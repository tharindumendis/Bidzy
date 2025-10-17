using Bidzy.API.DTOs.Admin;
using Bidzy.API.DTOs.auction;
using Bidzy.API.DTOs.bid;
using Bidzy.API.DTOs.user;

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
