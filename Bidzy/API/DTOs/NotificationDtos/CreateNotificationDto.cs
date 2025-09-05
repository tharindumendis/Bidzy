namespace Bidzy.API.DTOs.NotificationDtos
{
    public class CreateNotificationDto
    {
        public Guid UserId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Link { get; set; }
    }
}
