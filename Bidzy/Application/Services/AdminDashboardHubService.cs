using Bidzy.API.DTOs;
using Bidzy.API.DTOs.auctionDtos;
using Bidzy.API.DTOs.bidDtos;
using Bidzy.API.DTOs.userDtos;
using Bidzy.API.Hubs;
using Bidzy.Application.Services.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Bidzy.Application.Services
{
    public class AdminDashboardHubService(IHubContext<AuctionHub> hubContext, ILiveUserTracker liveUserTracker) : IAdminDashboardHubService
    {
        private readonly IHubContext<AuctionHub> hubContext = hubContext;
        private readonly ILiveUserTracker liveUserTracker = liveUserTracker;
        private const string AdminGroupName = "AdminDashboardGroup";

        public async Task BroadcastAnalyticsUpdate(SiteAnalyticsDto siteAnalyticsDto)
        {
            await hubContext.Clients.Group(AdminGroupName).SendAsync("ReceiveAnalyticsUpdate", siteAnalyticsDto);
        }

        public async Task BroadcastNewUser(UserReadDto userReadDto)
        {
            await hubContext.Clients.Group(AdminGroupName).SendAsync("ReceiveNewUser", userReadDto);
        }

        public async Task BroadcastAuctionUpdate(AuctionReadDto auctionReadDto)
        {
            await hubContext.Clients.Group(AdminGroupName).SendAsync("ReceiveAuctionUpdate", auctionReadDto);
        }

        public async Task BroadcastNewBid(BidReadDto bidReadDto)
        {
            await hubContext.Clients.Group(AdminGroupName).SendAsync("ReceiveNewBid", bidReadDto);
        }

        public async Task BroadcastLiveUsersUpdate()
        {
            await hubContext.Clients.Group(AdminGroupName).SendAsync("RegisteredUserCount", liveUserTracker.GetLiveUserCount());
        }
    }

}
