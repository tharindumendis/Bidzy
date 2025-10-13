using System.Collections.Concurrent;

namespace Bidzy.Application.Services.SignalR
{
    public class SignalRUserTracker : ILiveUserTracker
    {
        private readonly ConcurrentDictionary<string, List<string>> liveUsers = new();

        public Task UserConnected(string userId, string connectionId)
        {
            liveUsers.AddOrUpdate(userId, (key) => new List<string> { connectionId }, (key, existingConnections) =>
            {
                lock (existingConnections)
                {
                    existingConnections.Add(connectionId);
                }
                return existingConnections;
            });
            return Task.CompletedTask;
        }

        public Task UserDisconnected(string userId, string connectionId)
        {
            if (liveUsers.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        liveUsers.TryRemove(userId, out _);
                    }
                }
            }
            return Task.CompletedTask;
        }

        public int GetLiveUserCount()
        {
            return liveUsers.Count;
        }
    }
}
