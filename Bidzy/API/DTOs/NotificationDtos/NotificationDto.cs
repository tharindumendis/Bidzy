using Microsoft.Identity.Client;

namespace Bidzy.API.DTOs.NotificationDtos
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime SentAt {  get; set; }
        public bool IsSeen { get; set; }
    }
}
