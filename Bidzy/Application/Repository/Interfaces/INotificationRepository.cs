using Bidzy.API.DTOs;
using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(Guid userId);
        Task<IEnumerable<Notification>> GetUnseenNotificationsByUserIdAsync(Guid userId);
        Task<PagedResult<Notification>> GetNotificationsByUserIdAsync(Guid userId, int page, int pageSize);
        Task AddNotificationAsync (Notification notification);
        Task AddNotificationsAsync(List<Notification> notifications);
        Task MarkAsSeenAsync (Guid notificationId , Guid userId);
        Task MarkAllAsSeenByUserIdAsync (Guid userId);
        Task DeleteNotificationAsync (Guid notificationId);

    }
}
