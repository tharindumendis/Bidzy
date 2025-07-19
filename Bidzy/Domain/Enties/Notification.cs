using System.ComponentModel.DataAnnotations;
using Bidzy.Domain.Enum;

namespace Bidzy.Domain.Enties
{
    public class Notification
    {
        public Guid Id { get; set; }
        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string Message { get; set; }
        [Required]
        public NotificationType Type { get; set; } // Email, System
        public DateTime SentAt { get; set; }

    }
}
