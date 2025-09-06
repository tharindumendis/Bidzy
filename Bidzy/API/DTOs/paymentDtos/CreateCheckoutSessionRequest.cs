namespace Bidzy.API.DTOs.paymentDtos
{
    public class CreateCheckoutSessionRequest
    {
        public Guid AuctionId { get; set; }
        public string SuccessUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }
}

