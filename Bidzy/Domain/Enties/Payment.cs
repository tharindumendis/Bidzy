using System.ComponentModel.DataAnnotations;
using Bidzy.Domain.Enum;

namespace Bidzy.Domain.Enties
{
    public class Payment
    {
        public Guid Id { get; set; }
        [Required]
        public Guid BidId { get; set; }
        public Bid Bid { get; set; }
        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } // Nav via Bid.Bidder
        [Required]
        public decimal TotalAmount { get; set; }
        [Required]
        public decimal Commission { get; set; }
        // Stripe metadata
        public string? PaymentIntentId { get; set; }
        public string ChargeId { get; set; } = string.Empty;
        public string? Currency { get; set; }
        public decimal? AmountCaptured { get; set; }
        public decimal? ProcessorFee { get; set; }
        public decimal? NetAmount { get; set; }
        public string? ReceiptUrl { get; set; }
        public string? StatusReason { get; set; }
        [Required]
        public PaymentStatus Status { get; set; }
        [Required]
        public DateTime PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        // Refund fields
        public string? RefundId { get; set; }
        public decimal? RefundAmount { get; set; }
        public string? RefundStatus { get; set; }
        public DateTime? RefundedAt { get; set; }
    }
}
