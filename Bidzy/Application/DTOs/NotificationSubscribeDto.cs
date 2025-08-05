namespace Bidzy.Application.DTOs
{
    public class NotificationSubscribeDto
    {
        public string UserId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty; // e.g., auction ID
        public string Role { get; set; } = string.Empty;
        public string? SocketId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
