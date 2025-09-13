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
        public decimal TotalAmount { get; set; }
        [Required]
        public decimal Commission { get; set; }
        // Stripe metadata
        public string? PaymentIntentId { get; set; }
        public string ChargeId { get; set; }
        public string? Currency { get; set; }
        public decimal? AmountCaptured { get; set; }
        public decimal? ProcessorFee { get; set; }
        public decimal? NetAmount { get; set; }
        public string? ReceiptUrl { get; set; }
        public string? StatusReason { get; set; }
        [Required]
        public PaymentStatus Status { get; set; } // Pending, Completed, Failed
        [Required]
        public DateTime PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

    }
}
