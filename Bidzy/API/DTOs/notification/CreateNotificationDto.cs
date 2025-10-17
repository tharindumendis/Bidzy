namespace Bidzy.API.DTOs.notification
{
    public class CreateNotificationDto
    {
        public Guid UserId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Link { get; set; }
    }
}
