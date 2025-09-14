using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Settings;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;
using Microsoft.Extensions.Options;
using Bidzy.Data;
using Stripe;
using Stripe.Checkout;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Services.Payments
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOptions<StripeSettings> _settings;
        private readonly IBidRepository _bidRepository;
        private readonly ApplicationDbContext _db;
        private readonly INotificationRepository _nrepo;

        public StripePaymentService(IPaymentRepository paymentRepository, IBidRepository bidRepository, IOptions<StripeSettings> settings, ApplicationDbContext db, INotificationRepository nrepo)
        {
            _paymentRepository = paymentRepository;
            _bidRepository = bidRepository;
            _settings = settings;
            _db = db;
            _nrepo = nrepo;
            StripeConfiguration.ApiKey = _settings.Value.SecretKey;
        }

        public async Task<string> CreateCheckoutSessionForWinningBidAsync(
            Bid winningBid,
            decimal commissionRate,
            string currency,
            string successUrl,
            string cancelUrl)
        {
            // Price from server data
            var baseAmount = winningBid.Amount;
            var commission = Math.Round(baseAmount * commissionRate, 2, MidpointRounding.AwayFromZero);
            var total = baseAmount + commission;

            // amount in smallest currency unit (e.g., cents)
            long unitAmount = (long)Math.Round(total * 100m, 0, MidpointRounding.AwayFromZero);

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                ClientReferenceId = winningBid.Id.ToString(),
                Metadata = new Dictionary<string, string> { ["bidId"] = winningBid.Id.ToString() },
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = unitAmount,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Auction payment ({winningBid.AuctionId})"
                            }
                        }
                    }
                }
            };

            var service = new SessionService();
            // Idempotency per bid
            var requestOptions = new RequestOptions
            {
                IdempotencyKey = $"checkout:bid:{winningBid.Id}"
            };
            var session = await service.CreateAsync(options, requestOptions);

            // Note: We create Payment on webhook confirmation to keep DB consistent.
            return session.Url;
        }

        public async Task HandleWebhookAsync(string json, string signatureHeader)
        {
            var secret = _settings.Value.WebhookSecret;
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, secret);
            }
            catch
            {
                throw; // Let controller translate to 400
            }

            // Idempotent webhook processing by Event.Id
            var existingEvent = await _db.WebhookEventLogs.FindAsync(stripeEvent.Id);
            if (existingEvent != null)
            {
                return; // already processed
            }
            _db.WebhookEventLogs.Add(new Domain.Enties.WebhookEventLog { EventId = stripeEvent.Id, ReceivedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();

            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                if (session != null && session.ClientReferenceId != null)
                {
                    if (Guid.TryParse(session.ClientReferenceId, out var bidId))
                    {
                        // Create payment record if not exists
                        var existing = await _paymentRepository.GetByBidIdAsync(bidId);
                        if (existing == null)
                        {
                            // Stripe reports amount_total in smallest currency unit
                            var amountTotal = (session.AmountTotal ?? 0) / 100m;
                            var bid = await _bidRepository.GetBidByIdAsync(bidId);
                            var commission = bid != null ? Math.Max(0m, amountTotal - bid.Amount) : 0m;
                            // Attempt to fetch intent/charge details
                            string? currency = session.Currency;
                            string? paymentIntentId = session.PaymentIntentId;
                            string? chargeId = null;
                            string? receiptUrl = null;
                            decimal? amountCaptured = null;
                            try
                            {
                                if (!string.IsNullOrEmpty(paymentIntentId))
                                {
                                    var piService = new PaymentIntentService();
                                    var pi = await piService.GetAsync(paymentIntentId);
                                    amountCaptured = (pi?.AmountReceived ?? 0) / 100m;
                                    chargeId = pi?.LatestChargeId;
                                    if (!string.IsNullOrEmpty(chargeId))
                                    {
                                        var chargeService = new ChargeService();
                                        var ch = await chargeService.GetAsync(chargeId);
                                        receiptUrl = ch?.ReceiptUrl;
                                        currency ??= ch?.Currency;
                                    }
                                }
                            }
                            catch { }
                            var payment = new Payment
                            {
                                Id = Guid.NewGuid(),
                                BidId = bidId,
                                TotalAmount = amountTotal,
                                Commission = commission,
                                Currency = currency,
                                PaymentIntentId = paymentIntentId,
                                ChargeId = chargeId,
                                AmountCaptured = amountCaptured,
                                ReceiptUrl = receiptUrl,
                                Status = PaymentStatus.Completed,
                                PaidAt = DateTime.UtcNow
                            };
                            await _paymentRepository.AddAsync(payment);

                            // Create Delivery record (Pending) if not exists
                            if (bid != null)
                            {
                                var auc = await _db.Auctions.Include(a => a.Product).FirstOrDefaultAsync(a => a.Id == bid.AuctionId);
                                if (auc != null)
                                {
                                    var existingDelivery = await _db.Deliveries.FirstOrDefaultAsync(d => d.AuctionId == auc.Id);
                                    if (existingDelivery == null)
                                    {
                                        _db.Deliveries.Add(new Delivery
                                        {
                                            Id = Guid.NewGuid(),
                                            AuctionId = auc.Id,
                                            Status = DeliveryStatus.Pending,
                                            ShippedAt = DateTime.MinValue
                                        });
                                        await _db.SaveChangesAsync();
                                    }

                                    // Notifications to seller and buyer
                                    try
                                    {
                                        await _nrepo.AddNotificationAsync(new Notification
                                        {
                                            Id = Guid.NewGuid(),
                                            UserId = auc.Product.SellerId,
                                            Message = $"Payment received for '{auc.Product.Title}'.",
                                            Type = NotificationType.SYSTEM,
                                            Timestamp = DateTime.UtcNow,
                                            IsSeen = false
                                        });
                                        await _nrepo.AddNotificationAsync(new Notification
                                        {
                                            Id = Guid.NewGuid(),
                                            UserId = bid.BidderId,
                                            Message = $"Payment successful for '{auc.Product.Title}'.",
                                            Type = NotificationType.SYSTEM,
                                            Timestamp = DateTime.UtcNow,
                                            IsSeen = false
                                        });
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
            }
            else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
            {
                var pi = stripeEvent.Data.Object as PaymentIntent;
                // Optional: if pending payments are created, mark them failed here
            }
            else if (stripeEvent.Type == Events.CheckoutSessionExpired)
            {
                // Session expired without completion; nothing to persist since we create on success
            }
            // Optionally: handle refunds/disputes when model supports it
        }
    }
}
