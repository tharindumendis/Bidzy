using System.ComponentModel.DataAnnotations;
using Bidzy.Domain.Enum;

namespace Bidzy.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; }

        public string Message { get; set; }
        public string? Link { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        public DateTime? SeenAt { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsSeen { get; set; } = false;
    }
}
