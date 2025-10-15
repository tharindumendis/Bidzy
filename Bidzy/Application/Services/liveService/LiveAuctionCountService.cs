using System.Collections.Concurrent;
using System.Threading.Tasks;
using Bidzy.API.Hubs;
using Bidzy.Application.DTOs;
using Bidzy.Application.Services.LiveService;
using Microsoft.AspNetCore.SignalR;

public class LiveAuctionCountService(IHubContext<AuctionHub> hubContext, IHubContext<UserHub> userHubContext) : ILiveAuctionCountService
{
    private readonly IHubContext<AuctionHub> _hubContext = hubContext;
    private readonly IHubContext<UserHub> _userHubContext = userHubContext;
    private readonly ConcurrentDictionary<string, NotificationSubscribeDto> _connections = new();
    private readonly LiveCountDto _liveCount = new()
    {
        UserCount = 0,
        OngoingAuctionCount = 0,
        ScheduledAuctionCount = 0
    };

    public async Task AddConnection(string connectionId, NotificationSubscribeDto payload)
    {
        _connections[connectionId] = payload;
        _liveCount.UserCount = _connections.Count;
        await BroadcastLiveCountAsync();
    }

    public async Task RemoveConnection(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
        _liveCount.UserCount = _connections.Count;
        await BroadcastLiveCountAsync();
    }

    public async Task UpdateScheduledCount(int count)
    {
        _liveCount.ScheduledAuctionCount = count;
        await BroadcastLiveCountAsync();
    }
    public async Task AddScheduledCount(int count)
    {
        _liveCount.ScheduledAuctionCount = _liveCount.ScheduledAuctionCount + count;
        await BroadcastLiveCountAsync();
    }
    public async Task RemoveScheduledCount(int count)
    {
        _liveCount.ScheduledAuctionCount = _liveCount.ScheduledAuctionCount - count;
        await BroadcastLiveCountAsync();
    }

    public async Task UpdateOngoingCount(int count)
    {
        _liveCount.OngoingAuctionCount = count;
        await BroadcastLiveCountAsync();
    }
    public async Task AddOngoingCount(int count)
    {
        _liveCount.OngoingAuctionCount = _liveCount.OngoingAuctionCount + count;
        await BroadcastLiveCountAsync();
    }
    public async Task RemoveOngoingCount(int count)
    {
        _liveCount.OngoingAuctionCount = _liveCount.OngoingAuctionCount - count;
        await BroadcastLiveCountAsync();
    }

    public int GetUserCount() => _liveCount.UserCount?? 67676;

    public async Task BroadcastLiveCountAsync()
    {
        await _hubContext.Clients.Group("App").SendAsync("LiveCount", _liveCount);
        await _userHubContext.Clients.Group("App").SendAsync("LiveCount", _liveCount);

    }
    
}