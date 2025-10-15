using System.ComponentModel.DataAnnotations;

namespace Bidzy.API.DTOs.paymentDtos
{
    public class CreateCheckoutSessionRequest
    {
        public Guid AuctionId { get; set; }
        [Required]
        public string SuccessUrl { get; set; } = string.Empty;
        [Required]
        public string CancelUrl { get; set; } = string.Empty;
    }
}

