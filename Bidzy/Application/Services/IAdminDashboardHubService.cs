using Bidzy.API.DTOs;
using Bidzy.API.DTOs.adminDtos;
using Bidzy.API.DTOs.auctionDtos;
using Bidzy.API.DTOs.bidDtos;
using Bidzy.API.DTOs.userDtos;

namespace Bidzy.Application.Services
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
