using System.ComponentModel.DataAnnotations;

namespace Bidzy.Domain.Enties
{
    public class WebhookEventLog
    {
        [Key]
        public string EventId { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    }
}

