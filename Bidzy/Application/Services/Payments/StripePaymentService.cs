using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Settings;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;
using Microsoft.Extensions.Options;
using Bidzy.Data;
using Stripe;
using Stripe.Checkout;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Bidzy.Application.Services.Email;

namespace Bidzy.Application.Services.Payments
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOptions<StripeSettings> _settings;
        private readonly IBidRepository _bidRepository;
        private readonly ApplicationDbContext _db;
        private readonly INotificationRepository _nrepo;
        private readonly IEmailJobService _emailJobService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IPaymentRepository paymentRepository, IBidRepository bidRepository, IOptions<StripeSettings> settings, ApplicationDbContext db, INotificationRepository nrepo, IEmailJobService emailJobService, IUserRepository userRepository, ILogger<StripePaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _bidRepository = bidRepository;
            _settings = settings;
            _db = db;
            _nrepo = nrepo;
            _emailJobService = emailJobService;
            _userRepository = userRepository;
            _logger = logger;
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
            _logger.LogInformation("Received Stripe webhook. Signature: {SignatureHeader}", signatureHeader);
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, secret);
                _logger.LogInformation("Stripe webhook successfully constructed event of type: {EventType}", stripeEvent.Type);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error constructing Stripe event: {Message}", ex.Message);
                throw; // Let controller translate to 400
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error constructing Stripe event: {Message}", ex.Message);
                throw;
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
                _logger.LogInformation("Processing checkout.session.completed event.");
                var session = stripeEvent.Data.Object as Session;
                if (session != null && session.ClientReferenceId != null)
                {
                    if (Guid.TryParse(session.ClientReferenceId, out var bidId))
                    {
                        _logger.LogInformation("Checkout session completed for BidId: {BidId}", bidId);
                        // Create payment record if not exists
                        var existing = await _paymentRepository.GetByBidIdAsync(bidId);
                        if (existing == null)
                        {
                            _logger.LogInformation("Creating new payment record for BidId: {BidId}", bidId);
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
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error fetching payment intent/charge details for PaymentIntentId: {PaymentIntentId}", paymentIntentId);
                            }
                            var payment = new Payment
                            {
                                Id = Guid.NewGuid(),
                                BidId = bidId,
                                UserId = bid != null ? bid.BidderId : Guid.Empty,
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
                            _logger.LogInformation("Payment record created for BidId: {BidId}", bidId);

                            // Create Delivery record (Pending) if not exists
                            if (bid != null)
                            {
                                var auc = await _db.Auctions.Include(a => a.Product).FirstOrDefaultAsync(a => a.Id == bid.AuctionId);
                                if (auc != null)
                                {
                                    var existingDelivery = await _db.Deliveries.FirstOrDefaultAsync(d => d.AuctionId == auc.Id);
                                    if (existingDelivery == null)
                                    {
                                        _logger.LogInformation("Creating new delivery record for AuctionId: {AuctionId}", auc.Id);
                                        _db.Deliveries.Add(new Delivery
                                        {
                                            Id = Guid.NewGuid(),
                                            AuctionId = auc.Id,
                                            Status = DeliveryStatus.Pending,
                                            ShippedAt = DateTime.MinValue
                                        });
                                        await _db.SaveChangesAsync();
                                        _logger.LogInformation("Delivery record created for AuctionId: {AuctionId}", auc.Id);
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
                                        _logger.LogInformation("Notifications sent for payment completion for AuctionId: {AuctionId}", auc.Id);

                                        // Send payment receipt email
                                        var buyer = await _userRepository.GetUserByIdAsync(bid.BidderId);
                                        if (buyer != null)
                                        {
                                            await _emailJobService.SendPaymentReceiptEmail(payment, buyer, auc);
                                            _logger.LogInformation("Payment receipt email sent to buyer {BuyerEmail} for AuctionId: {AuctionId}", buyer.Email, auc.Id);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error sending notifications or email for payment completion for AuctionId: {AuctionId}", auc.Id);
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Payment record already exists for BidId: {BidId}. Status: {Status}", bidId, existing.Status);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid BidId in ClientReferenceId: {ClientReferenceId}", session.ClientReferenceId);
                    }
                }
                else
                {
                    _logger.LogWarning("Session or ClientReferenceId is null for checkout.session.completed event.");
                }
            }
            else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
            {
                _logger.LogWarning("Processing payment_intent.payment_failed event.");
                var pi = stripeEvent.Data.Object as PaymentIntent;
                if (pi != null && pi.Metadata != null && pi.Metadata.TryGetValue("bidId", out var bidIdString) && Guid.TryParse(bidIdString, out var bidId))
                {
                    var payment = await _paymentRepository.GetByBidIdAsync(bidId);
                    if (payment != null)
                    {
                        payment.Status = PaymentStatus.Failed;
                        payment.StatusReason = pi.LastPaymentError?.Message ?? "Payment failed";
                        payment.UpdatedAt = DateTime.UtcNow;
                        await _paymentRepository.UpdateAsync(payment);
                        _logger.LogInformation("Payment record for BidId: {BidId} updated to Failed. Reason: {Reason}", bidId, payment.StatusReason);

                        var bid = await _bidRepository.GetBidByIdAsync(bidId);
                        if (bid != null)
                        {
                            var auction = await _db.Auctions.Include(a => a.Product).FirstOrDefaultAsync(a => a.Id == bid.AuctionId);
                            var buyer = await _userRepository.GetUserByIdAsync(bid.BidderId);
                            if (buyer != null && auction != null)
                            {
                                await _emailJobService.SendPaymentFailedEmail(payment, buyer, auction, payment.StatusReason);
                                _logger.LogInformation("Payment failed email sent to buyer {BuyerEmail} for AuctionId: {AuctionId}", buyer.Email, auction.Id);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("PaymentIntent {PaymentIntentId} failed, but no associated BidId found in metadata or payment record.", pi?.Id);
                }
            }
            else if (stripeEvent.Type == Events.CheckoutSessionExpired)
            {
                _logger.LogWarning("Processing checkout.session.expired event.");
                var session = stripeEvent.Data.Object as Session;
                if (session != null && session.ClientReferenceId != null && Guid.TryParse(session.ClientReferenceId, out var bidId))
                {
                    var payment = await _paymentRepository.GetByBidIdAsync(bidId);
                    if (payment != null && payment.Status == PaymentStatus.Pending)
                    {
                        payment.Status = PaymentStatus.Failed;
                        payment.StatusReason = "Checkout session expired";
                        payment.UpdatedAt = DateTime.UtcNow;
                        await _paymentRepository.UpdateAsync(payment);
                        _logger.LogInformation("Payment record for BidId: {BidId} updated to Failed due to session expiration.", bidId);

                        var bid = await _bidRepository.GetBidByIdAsync(bidId);
                        if (bid != null)
                        {
                            var auction = await _db.Auctions.Include(a => a.Product).FirstOrDefaultAsync(a => a.Id == bid.AuctionId);
                            var buyer = await _userRepository.GetUserByIdAsync(bid.BidderId);
                            if (buyer != null && auction != null)
                            {
                                await _emailJobService.SendPaymentFailedEmail(payment, buyer, auction, payment.StatusReason);
                                _logger.LogInformation("Payment failed email sent to buyer {BuyerEmail} for AuctionId: {AuctionId} due to session expiration.", buyer.Email, auction.Id);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Checkout session expired, but no associated BidId found in ClientReferenceId or no pending payment record.");
                }
            }
            else if (stripeEvent.Type == Events.ChargeRefunded)
            {
                _logger.LogInformation("Processing charge.refunded event.");
                var charge = stripeEvent.Data.Object as Charge;
                if (charge != null)
                {
                    var payment = await _paymentRepository.GetByChargeIdAsync(charge.Id);
                    if (payment != null)
                    {
                        payment.RefundId = charge.Refunds?.Data?.LastOrDefault()?.Id;
                        payment.RefundAmount = charge.AmountRefunded / 100m;
                        payment.RefundedAt = DateTime.UtcNow;
                        payment.RefundStatus = "Refunded";
                        payment.Status = PaymentStatus.Refunded;
                        payment.UpdatedAt = DateTime.UtcNow;
                        await _paymentRepository.UpdateAsync(payment);

                        var bid = await _bidRepository.GetBidByIdAsync(payment.BidId);
                        if (bid != null && !bid.IsRefunded)
                        {
                            bid.IsRefunded = true;
                            await _bidRepository.UpdateBidAsync(bid);

                            try
                            {
                                var auction = await _db.Auctions.Include(a => a.Product).FirstOrDefaultAsync(a => a.Id == bid.AuctionId);
                                var buyer = await _userRepository.GetUserByIdAsync(bid.BidderId);
                                if (auction != null)
                                {
                                    await _nrepo.AddNotificationAsync(new Notification
                                    {
                                        Id = Guid.NewGuid(),
                                        UserId = auction.Product.SellerId,
                                        Message = $"Payment refunded for '{auction.Product.Title}'.",
                                        Type = NotificationType.PAYMENTREFUNDED,
                                        Timestamp = DateTime.UtcNow,
                                        IsSeen = false
                                    });
                                }
                                if (buyer != null)
                                {
                                    await _nrepo.AddNotificationAsync(new Notification
                                    {
                                        Id = Guid.NewGuid(),
                                        UserId = buyer.Id,
                                        Message = "Your payment refund has been processed.",
                                        Type = NotificationType.PAYMENTREFUNDED,
                                        Timestamp = DateTime.UtcNow,
                                        IsSeen = false
                                    });
                                }

                                if (auction != null && buyer != null)
                                {
                                    await _emailJobService.SendRefundReceiptEmail(payment, buyer, auction);
                                    await _emailJobService.SendRefundNotificationEmail(payment, auction.Product.Seller, auction);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error sending refund notifications/emails for BidId: {BidId}", bid.Id);
                            }
                        }
                    }
                }
            }
            
            else
            {
                _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
            }
            // Optionally: extend additional events here in future
        }

        public async Task<Payment> CreateRefundAsync(Guid paymentId, Guid userId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId) ?? throw new KeyNotFoundException("Payment not found");
            if (payment.Status != PaymentStatus.Completed)
                throw new InvalidOperationException("Only completed payments can be refunded.");

            var bid = await _bidRepository.GetBidByIdAsync(payment.BidId) ?? throw new InvalidOperationException("Associated bid not found.");
            if (bid.BidderId != userId)
                throw new UnauthorizedAccessException("You are not allowed to refund this payment.");

            var options = new RefundCreateOptions();
            if (!string.IsNullOrEmpty(payment.PaymentIntentId))
            {
                options.PaymentIntent = payment.PaymentIntentId;
            }
            else if (!string.IsNullOrEmpty(payment.ChargeId))
            {
                options.Charge = payment.ChargeId;
            }
            else
            {
                throw new InvalidOperationException("Payment has no PaymentIntentId or ChargeId.");
            }

            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(options, new RequestOptions
            {
                IdempotencyKey = $"refund:{payment.Id}:full"
            });

            // Update payment and bid for full refund
            payment.RefundId = refund.Id;
            payment.RefundAmount = refund.Amount / 100m;
            payment.RefundedAt = DateTime.UtcNow;
            payment.RefundStatus = "Refunded";
            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);

            bid.IsRefunded = true;
            await _bidRepository.UpdateBidAsync(bid);

            try
            {
                var auction = await _db.Auctions.Include(a => a.Product).FirstOrDefaultAsync(a => a.Id == bid.AuctionId);
                var buyer = await _userRepository.GetUserByIdAsync(bid.BidderId);
                if (auction != null)
                {
                    await _nrepo.AddNotificationAsync(new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = auction.Product.SellerId,
                        Message = $"Payment refunded for '{auction.Product.Title}'.",
                        Type = NotificationType.PAYMENTREFUNDED,
                        Timestamp = DateTime.UtcNow,
                        IsSeen = false
                    });
                }
                if (buyer != null)
                {
                    await _nrepo.AddNotificationAsync(new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = buyer.Id,
                        Message = "Your payment refund has been processed.",
                        Type = NotificationType.PAYMENTREFUNDED,
                        Timestamp = DateTime.UtcNow,
                        IsSeen = false
                    });
                }

                if (auction != null && buyer != null)
                {
                    await _emailJobService.SendRefundReceiptEmail(payment, buyer, auction);
                    await _emailJobService.SendRefundNotificationEmail(payment, auction.Product.Seller, auction);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund notifications/emails for PaymentId: {PaymentId}", payment.Id);
            }

            return payment;
        }

        
    }
}
