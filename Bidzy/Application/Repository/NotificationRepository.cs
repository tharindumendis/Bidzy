using Bidzy.API.DTOs;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext dbContext;

        public NotificationRepository (ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task <IEnumerable<Notification>> GetNotificationsByUserIdAsync (Guid userId)
        {
            return await dbContext.Notifications
                .Where ( u => u.UserId == userId)
                .OrderByDescending ( u => u.Timestamp )
                .ToListAsync ();
        }
        public async Task<PagedResult<Notification>> GetNotificationsByUserIdAsync(Guid userId, int page, int pageSize)
        {
            var query = dbContext.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.Timestamp);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Notification>
            {
                TotalCount = totalCount,
                Items = items
            };
        }

        public async Task<IEnumerable<Notification>> GetUnseenNotificationsByUserIdAsync(Guid userId)
        {
            return await dbContext.Notifications
                .Where( u => u.UserId == userId && !u.IsSeen)
                .OrderByDescending( u => u.Timestamp )
                .ToListAsync ();
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            await dbContext.Notifications.AddAsync(notification);
            await dbContext.SaveChangesAsync ();
        }
        public async Task AddNotificationsAsync(List<Notification> notifications)
        {
            await dbContext.Notifications.AddRangeAsync (notifications);
            await dbContext.SaveChangesAsync();
        }

        public async Task MarkAsSeenAsync(Guid notificationId, Guid userId)
        {
            var notification = await dbContext.Notifications.FindAsync (notificationId);
            if (notification != null && notification.UserId == userId)
            {

                notification.IsSeen = true;
                await dbContext.SaveChangesAsync ();
            }
        }

        public async Task MarkAllAsSeenByUserIdAsync(Guid userId)
        {
            var notification = await dbContext.Notifications
                .Where(u => u.UserId == userId && !u.IsSeen)
                .ToListAsync ();

            foreach (var n in notification)
            {
                n.IsSeen = true;
            }
            await dbContext.SaveChangesAsync ();
        }

        public async Task DeleteNotificationAsync (Guid notificationId)
        {
            var notification = await dbContext.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                dbContext.Notifications.Remove(notification);
                await dbContext.SaveChangesAsync ();
            }
        }
        // remove seen notifications older than a certain date
        public async Task DeleteOutdatedSeenNotificationsAsync(DateTime threshold)
        {
            var outdatedNotifications = await dbContext.Notifications
                .Where(n => n.Timestamp < threshold)
                .Where(n => n.IsSeen) // Only delete seen notifications
                .ToListAsync();
            if (outdatedNotifications.Count != 0)
            {
                dbContext.Notifications.RemoveRange(outdatedNotifications);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
