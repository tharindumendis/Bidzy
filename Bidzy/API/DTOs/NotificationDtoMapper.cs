using Bidzy.API.DTOs.NotificationDtos;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;

namespace Bidzy.API.DTOs
{
    public class NotificationDtoMapper
    {
        public static NotificationDto ToDto (Notification entity)
        {
            return new NotificationDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Message = entity.Message,
                Type = entity.Type.ToString(),
                Timestamp = entity.Timestamp,
                IsSeen = entity.IsSeen,
            };
        }

        public static Notification ToEntity (CreateNotificationDto dto)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Message = dto.Message,
                Type = Enum.Parse<NotificationType>(dto.Type, true),
                Link = dto.Link,
                IsSeen = false,
            };
        }
    }
}
