using System.ComponentModel.DataAnnotations;
using Bidzy.Domain.Enum;

namespace Bidzy.Domain.Enties
{
    public class Delivery
    {
        public Guid Id { get; set; }
        [Required]
        public Guid AuctionId { get; set; }
        public Auction Auction { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        [Required]
        public DeliveryStatus Status { get; set; }
    }
}
