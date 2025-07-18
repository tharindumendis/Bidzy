using System.ComponentModel.DataAnnotations;

namespace Bidzy.Modles.Enties
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
        [Required]
        public PaymentStatus Status { get; set; } // Pending, Completed, Failed
        [Required]
        public DateTime PaidAt { get; set; }

    }
}
