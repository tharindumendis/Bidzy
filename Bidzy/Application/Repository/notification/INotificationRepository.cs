using Bidzy.API.DTOs.Common;
using Bidzy.Domain.Entities;

namespace Bidzy.Application.Repository.Notification
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Domain.Entities.Notification>> GetNotificationsByUserIdAsync(Guid userId);
        Task<IEnumerable<Domain.Entities.Notification>> GetUnseenNotificationsByUserIdAsync(Guid userId);
        Task<PagedResult<Domain.Entities.Notification>> GetNotificationsByUserIdAsync(Guid userId, int page, int pageSize);
        Task AddNotificationAsync (Domain.Entities.Notification notification);
        Task AddNotificationsAsync(List<Domain.Entities.Notification> notifications);
        Task MarkAsSeenAsync (Guid notificationId , Guid userId);
        Task MarkAllAsSeenByUserIdAsync (Guid userId);
        Task DeleteNotificationAsync (Guid notificationId);

    }
}
