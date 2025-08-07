using System.Collections.Concurrent;
using System.Threading.Tasks;
using Bidzy.API.Hubs;
using Bidzy.Application.DTOs;
using Bidzy.Application.Services;
using Microsoft.AspNetCore.SignalR;

public class LiveAuctionCountService : ILiveAuctionCountService
{
    private readonly IHubContext<UserHub> _hubContext;
    private readonly ConcurrentDictionary<string, NotificationSubscribeDto> _connections = new();
    private readonly LiveCountDto _liveCount = new()
    {
        UserCount = 0,
        OngoingAuctionCount = 0,
        ScheduledAuctionCount = 0
    };

    public LiveAuctionCountService(IHubContext<UserHub> hubContext)
    {
        _hubContext = hubContext;
    }

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
        await _hubContext.Clients.Group("LiveCount").SendAsync("LiveCount", _liveCount);
    }
}