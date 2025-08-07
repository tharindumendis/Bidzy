using Bidzy.Application.DTOs;

namespace Bidzy.Application.Services
{
    public interface ILiveAuctionCountService
    {
        Task UpdateScheduledCount(int count);
        Task AddScheduledCount(int count);
        Task RemoveScheduledCount(int count);
        Task UpdateOngoingCount(int count);
        Task AddOngoingCount(int count);
        Task RemoveOngoingCount(int count);
        Task AddConnection(string connectionId, NotificationSubscribeDto payload);
        Task RemoveConnection(string connectionId);
        int GetUserCount();
        Task BroadcastLiveCountAsync();

    }
}
