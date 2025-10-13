namespace Bidzy.Application.Services.SignalR
{
    public interface ILiveUserTracker
    {
        Task UserConnected(string userId, string connectionId);
        Task UserDisconnected(string userId, string connectionId);
        int GetLiveUserCount();
    }
}
