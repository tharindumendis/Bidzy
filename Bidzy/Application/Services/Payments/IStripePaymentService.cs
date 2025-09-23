using Bidzy.Domain.Enties;

namespace Bidzy.Application.Services.Payments
{
    public interface IStripePaymentService
    {
        Task<string> CreateCheckoutSessionForWinningBidAsync(Bid winningBid, decimal commissionRate, string currency, string successUrl, string cancelUrl);
        Task HandleWebhookAsync(string json, string signatureHeader);
        Task<Payment> CreateRefundAsync(Guid paymentId, Guid userId);
        Task EnrichPaymentAsync(Guid bidId, string? paymentIntentId);
    }
}
