namespace Bidzy.API.DTOs.NotificationDtos
{
    public class NotificationSeenDto
    {
        public string NotificationId { get; set; }
        public required string UserId { get; set; } 
    }
}
