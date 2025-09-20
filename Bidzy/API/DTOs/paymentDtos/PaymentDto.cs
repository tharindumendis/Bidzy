namespace Bidzy.API.DTOs.paymentDtos
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid BidId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Commission { get; set; }
        public string? Currency { get; set; }
        public decimal? AmountCaptured { get; set; }
        public decimal? ProcessorFee { get; set; }
        public decimal? NetAmount { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? ChargeId { get; set; }
        public string? ReceiptUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? RefundId { get; set; }
        public decimal? RefundAmount { get; set; }
        public string? RefundStatus { get; set; }
        public DateTime? RefundedAt { get; set; }
    }
}
