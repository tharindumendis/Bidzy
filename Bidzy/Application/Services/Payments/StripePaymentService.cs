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
using System.Text.Json;
using System.Collections.Generic;
using Hangfire;

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

        public async Task EnrichPaymentAsync(Guid bidId, string? paymentIntentId)
        {
            try
            {
                Payment? payment = null;
                if (bidId != Guid.Empty)
                {
                    payment = await _paymentRepository.GetByBidIdAsync(bidId);
                }
                if (payment == null && !string.IsNullOrEmpty(paymentIntentId))
                {
                    var piService = new PaymentIntentService();
                    var pi = await piService.GetAsync(paymentIntentId);
                    if (pi.Metadata != null && pi.Metadata.TryGetValue("bidId", out var bidIdString) && Guid.TryParse(bidIdString, out var resolvedBidId))
                    {
                        payment = await _paymentRepository.GetByBidIdAsync(resolvedBidId);
                    }
                }
                if (payment == null)
                {
                    _logger.LogWarning("[StripeEnrich] Payment not found for enrichment. bidId={BidId} pi={PI}", bidId, paymentIntentId);
                    return;
                }

                if (!string.IsNullOrEmpty(payment.PaymentIntentId)
                    && !string.IsNullOrEmpty(payment.ChargeId)
                    && payment.AmountCaptured.HasValue
                    && payment.ProcessorFee.HasValue
                    && payment.NetAmount.HasValue
                    && !string.IsNullOrEmpty(payment.ReceiptUrl))
                {
                    _logger.LogDebug("[StripeEnrich] Payment {PaymentId} already enriched. Skipping.", payment.Id);
                    return;
                }

                var details = await FetchStripePaymentDetailsAsync(paymentIntentId ?? payment.PaymentIntentId, payment.Currency);
                payment.Currency = details.currency ?? payment.Currency;
                payment.PaymentIntentId = details.paymentIntentId ?? payment.PaymentIntentId;
                payment.ChargeId = details.chargeId ?? payment.ChargeId;
                payment.ReceiptUrl = details.receiptUrl ?? payment.ReceiptUrl;
                payment.AmountCaptured = details.amountCaptured ?? payment.AmountCaptured;
                payment.ProcessorFee = details.processorFee ?? payment.ProcessorFee;
                payment.NetAmount = details.netAmount ?? payment.NetAmount;
                if (payment.Status != PaymentStatus.Completed)
                {
                    payment.Status = PaymentStatus.Completed;
                    payment.PaidAt = DateTime.UtcNow;
                }
                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);
                _logger.LogInformation("[StripeEnrich] Payment {PaymentId} enriched successfully.", payment.Id);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "[StripeEnrich] Stripe API error. bidId={BidId} pi={PI}", bidId, paymentIntentId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StripeEnrich] Error enriching payment. bidId={BidId} pi={PI}", bidId, paymentIntentId);
                throw;
            }
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
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["bidId"] = winningBid.Id.ToString()
                    }
                },
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
            var session = await service.CreateAsync(options);

            // Note: We create Payment on webhook confirmation to keep DB consistent.
            return session.Url;
        }

        public async Task HandleWebhookAsync(string json, string signatureHeader)
        {
            var secret = _settings.Value.WebhookSecret;
            Event stripeEvent;
            var skipSig = _settings.Value.SkipSignatureVerification;
            _logger.LogDebug("[StripeWebhook] Payload received. skipVerify={Skip} length={Length}", skipSig, json?.Length ?? 0);
            try
            {
                if (skipSig || string.IsNullOrWhiteSpace(secret))
                {
                    _logger.LogWarning("[StripeWebhook] Skipping signature verification (development mode).");
                    stripeEvent = EventUtility.ParseEvent(json);
                }
                else
                {
                    stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, secret, tolerance: _settings.Value.WebhookToleranceSeconds);
                }
                _logger.LogInformation("[StripeWebhook] Constructed event. id={EventId} type={EventType}", stripeEvent.Id, stripeEvent.Type);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "[StripeWebhook] Error constructing Stripe event: {Message}", ex.Message);
                if (skipSig)
                {
                    _logger.LogWarning("[StripeWebhook] Falling back to dev-lite processing path due to parse error.");
                    var handled = await TryHandleWebhookLiteAsync(json);
                    if (handled) return; // already processed + logged
                }
                throw; // Let controller translate to 400
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StripeWebhook] Unexpected error constructing Stripe event: {Message}", ex.Message);
                if (skipSig)
                {
                    _logger.LogWarning("[StripeWebhook] Falling back to dev-lite processing path due to parse error.");
                    var handled = await TryHandleWebhookLiteAsync(json);
                    if (handled) return;
                }
                throw;
            }

            // Idempotent webhook processing by Event.Id (log after successful processing)
            var existingEvent = await _db.WebhookEventLogs.FindAsync(stripeEvent.Id);
            if (existingEvent != null)
            {
                _logger.LogInformation("[StripeWebhook] Event {EventId} already processed. Skipping.", stripeEvent.Id);
                return; // already processed
            }

            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
            {
                _logger.LogInformation("[StripeWebhook] Handling {EventType}", Events.CheckoutSessionCompleted);
                var session = stripeEvent.Data.Object as Session;
                if (session != null && session.ClientReferenceId != null)
                {
                    if (Guid.TryParse(session.ClientReferenceId, out var bidId))
                    {
                        _logger.LogInformation("[StripeWebhook] Session completed. sessionId={SessionId} bidId={BidId} amount_total={AmountTotal} currency={Currency}", session.Id, bidId, session.AmountTotal, session.Currency);
                        // Upsert payment record
                        var existing = await _paymentRepository.GetByBidIdAsync(bidId);
                        // Stripe reports amount_total in smallest currency unit
                        var amountTotal = (session.AmountTotal ?? 0) / 100m;
                        var bid = await _bidRepository.GetBidByIdAsync(bidId);
                        var commission = bid != null ? Math.Max(0m, amountTotal - bid.Amount) : 0m;
                        // Minimal fields from session; defer enrichment to background job for speed
                        string? currency = session.Currency;
                        string? paymentIntentId = session.PaymentIntentId;
                        string? chargeId = null;
                        string? receiptUrl = null;
                        decimal? amountCaptured = null;
                        Payment? finalPayment = null;
                        if (existing == null)
                        {
                            _logger.LogInformation("[StripeWebhook] Creating new Payment for bidId={BidId}", bidId);
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
                            finalPayment = payment;
                            _logger.LogInformation("[StripeWebhook] Payment created id={PaymentId} for bidId={BidId}", payment.Id, bidId);
                        }
                        else if (existing.Status != PaymentStatus.Completed)
                        {
                            _logger.LogInformation("[StripeWebhook] Updating existing Payment id={PaymentId} to Completed for bidId={BidId}", existing.Id, bidId);
                            existing.TotalAmount = amountTotal;
                            existing.Commission = commission;
                            existing.Currency = currency;
                            existing.PaymentIntentId = paymentIntentId ?? existing.PaymentIntentId;
                            existing.ChargeId = chargeId ?? existing.ChargeId;
                            existing.AmountCaptured = amountCaptured;
                            existing.ReceiptUrl = receiptUrl;
                            existing.Status = PaymentStatus.Completed;
                            existing.PaidAt = DateTime.UtcNow;
                            existing.UpdatedAt = DateTime.UtcNow;
                            await _paymentRepository.UpdateAsync(existing);
                            finalPayment = existing;
                        }
                        else
                        {
                            _logger.LogInformation("[StripeWebhook] Payment already Completed id={PaymentId} bidId={BidId}", existing.Id, bidId);
                            finalPayment = existing;
                        }

                        // Enqueue enrichment job to fetch PI/Charge/Balance details without blocking webhook
                        if (!string.IsNullOrEmpty(paymentIntentId))
                        {
                            BackgroundJob.Enqueue<IStripePaymentService>(s => s.EnrichPaymentAsync(bidId, paymentIntentId));
                            _logger.LogInformation("[StripeWebhook] Enrichment job enqueued for bidId={BidId} PI={PaymentIntentId}", bidId, paymentIntentId);
                        }

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
                                        ShippedAt = null
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
                                    _logger.LogInformation("[StripeWebhook] Notifications queued/sent for AuctionId={AuctionId}", auc.Id);

                                    // Send payment receipt email
                                    var buyer = await _userRepository.GetUserByIdAsync(bid.BidderId);
                                    if (buyer != null)
                                    {
                                        if (finalPayment != null)
                                        {
                                            await _emailJobService.SendPaymentReceiptEmail(finalPayment, buyer, auc);
                                        }
                                        _logger.LogInformation("Payment receipt email sent to buyer {BuyerEmail} for AuctionId: {AuctionId}", buyer.Email, auc.Id);
                                    }

                                    if (auc.Product?.SellerId != Guid.Empty)
                                    {
                                        var sellerUser = await _userRepository.GetUserByIdAsync(auc.Product.SellerId);
                                        if (sellerUser != null && finalPayment != null)
                                        {
                                            await _emailJobService.SendPaymentReceiptSellerEmail(finalPayment, sellerUser, buyer, auc);
                                            _logger.LogInformation("Payment receipt email sent to seller {SellerEmail} for AuctionId: {AuctionId}", sellerUser.Email, auc.Id);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "[StripeWebhook] Error sending notifications/emails for AuctionId={AuctionId}", auc.Id);
                                }
                            }
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
            else if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                _logger.LogInformation("[StripeWebhook] Handling {EventType}", Events.PaymentIntentSucceeded);
                var pi = stripeEvent.Data.Object as PaymentIntent;
                if (pi != null)
                {
                    if (pi.Metadata != null && pi.Metadata.TryGetValue("bidId", out var bidIdString) && Guid.TryParse(bidIdString, out var bidId))
                    {
                        BackgroundJob.Enqueue<IStripePaymentService>(s => s.EnrichPaymentAsync(bidId, pi.Id));
                        _logger.LogInformation("[StripeWebhook] Enrichment job enqueued from PI.succeeded for bidId={BidId} PI={PaymentIntentId}", bidId, pi.Id);
                    }
                }
            }
            else if (stripeEvent.Type == Events.ChargeSucceeded)
            {
                _logger.LogInformation("[StripeWebhook] Handling {EventType}", Events.ChargeSucceeded);
                var ch = stripeEvent.Data.Object as Charge;
                if (ch != null)
                {
                    if (!string.IsNullOrEmpty(ch.PaymentIntentId))
                    {
                        // Enqueue by PI id; enrichment will update charge-related fields too
                        var piId = ch.PaymentIntentId;
                        try
                        {
                            // Get bidId from PI metadata synchronously might block; skip and let job handle lookup
                            BackgroundJob.Enqueue<IStripePaymentService>(s => s.EnrichPaymentAsync(Guid.Empty, piId));
                            _logger.LogInformation("[StripeWebhook] Enrichment job enqueued from charge.succeeded for PI={PaymentIntentId}", piId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[StripeWebhook] Failed to enqueue enrichment from charge.succeeded");
                        }
                    }
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
                _logger.LogInformation("[StripeWebhook] Unhandled event type: {EventType}", stripeEvent.Type);
            }
            // Mark webhook event as processed only after successful handling
            _db.WebhookEventLogs.Add(new Domain.Enties.WebhookEventLog { EventId = stripeEvent.Id, ReceivedAt = DateTime.UtcNow });
            _logger.LogInformation("[StripeWebhook] Marking event {EventId} as processed.", stripeEvent.Id);
            await _db.SaveChangesAsync();
            _logger.LogInformation("[StripeWebhook] Event {EventId} processed successfully.", stripeEvent.Id);
            // Optionally: extend additional events here in future
        }

        private async Task<bool> TryHandleWebhookLiteAsync(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var type = root.TryGetProperty("type", out var tEl) ? tEl.GetString() : null;
                var id = root.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
                if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("[StripeWebhook-lite] Missing type or id; cannot process.");
                    return false;
                }

                // Idempotency check
                var existingEvent = await _db.WebhookEventLogs.FindAsync(id);
                if (existingEvent != null)
                {
                    _logger.LogInformation("[StripeWebhook-lite] Event {EventId} already processed. Skipping.", id);
                    return true;
                }

                if (type == "checkout.session.completed")
                {
                    if (!root.TryGetProperty("data", out var dataEl) || !dataEl.TryGetProperty("object", out var objEl))
                    {
                        _logger.LogWarning("[StripeWebhook-lite] No data.object for session.completed.");
                        return false;
                    }

                    var clientRef = objEl.TryGetProperty("client_reference_id", out var cr) ? cr.GetString() : null;
                    if (!Guid.TryParse(clientRef, out var bidId))
                    {
                        _logger.LogWarning("[StripeWebhook-lite] Invalid client_reference_id: {ClientRef}", clientRef);
                        return false;
                    }

                    var amountTotalCents = objEl.TryGetProperty("amount_total", out var amtEl) ? (amtEl.ValueKind == JsonValueKind.Number ? amtEl.GetInt64() : 0L) : 0L;
                    var currency = objEl.TryGetProperty("currency", out var curEl) ? curEl.GetString() : null;
                    var paymentIntentId = objEl.TryGetProperty("payment_intent", out var piEl) ? piEl.GetString() : null;

                    var existing = await _paymentRepository.GetByBidIdAsync(bidId);
                    var bid = await _bidRepository.GetBidByIdAsync(bidId);
                    var amountTotal = amountTotalCents / 100m;
                    var commission = bid != null ? Math.Max(0m, amountTotal - bid.Amount) : 0m;

                    if (existing == null)
                    {
                        _logger.LogInformation("[StripeWebhook-lite] Creating Payment for bid {BidId}", bidId);
                        var payment = new Payment
                        {
                            Id = Guid.NewGuid(),
                            BidId = bidId,
                            UserId = bid != null ? bid.BidderId : Guid.Empty,
                            TotalAmount = amountTotal,
                            Commission = commission,
                            Currency = currency,
                            PaymentIntentId = paymentIntentId,
                            Status = PaymentStatus.Completed,
                            PaidAt = DateTime.UtcNow
                        };
                        await _paymentRepository.AddAsync(payment);
                    }
                    else if (existing.Status != PaymentStatus.Completed)
                    {
                        _logger.LogInformation("[StripeWebhook-lite] Updating Payment {PaymentId} to Completed for bid {BidId}", existing.Id, bidId);
                        existing.TotalAmount = amountTotal;
                        existing.Commission = commission;
                        existing.Currency = currency;
                        existing.PaymentIntentId = paymentIntentId ?? existing.PaymentIntentId;
                        existing.Status = PaymentStatus.Completed;
                        existing.PaidAt = DateTime.UtcNow;
                        existing.UpdatedAt = DateTime.UtcNow;
                        await _paymentRepository.UpdateAsync(existing);
                    }
                    // Delivery + notifications minimal (reuse same logic as above would require more mapping)
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
                                    ShippedAt = null
                                });
                                await _db.SaveChangesAsync();
                            }
                        }
                    }

                    // Enqueue enrichment job; we may not have bidId resolved when parsing PI only, but we include it when we do
                    if (Guid.TryParse(clientRef, out var parsedBidId))
                    {
                        BackgroundJob.Enqueue<IStripePaymentService>(s => s.EnrichPaymentAsync(parsedBidId, paymentIntentId));
                        _logger.LogInformation("[StripeWebhook-lite] Enrichment job enqueued for bidId={BidId} PI={PaymentIntentId}", parsedBidId, paymentIntentId);
                    }
                    _db.WebhookEventLogs.Add(new WebhookEventLog { EventId = id, ReceivedAt = DateTime.UtcNow });
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("[StripeWebhook-lite] Event {EventId} processed successfully.", id);
                    return true;
                }
                else if (type == "payment_intent.payment_failed")
                {
                    if (!root.TryGetProperty("data", out var dataEl) || !dataEl.TryGetProperty("object", out var objEl))
                    {
                        _logger.LogWarning("[StripeWebhook-lite] No data.object for payment_failed.");
                        return false;
                    }
                    Guid bidId = Guid.Empty;
                    if (objEl.TryGetProperty("metadata", out var mdEl) && mdEl.ValueKind == JsonValueKind.Object)
                    {
                        if (mdEl.TryGetProperty("bidId", out var bidEl)) Guid.TryParse(bidEl.GetString(), out bidId);
                    }
                    if (bidId == Guid.Empty)
                    {
                        _logger.LogWarning("[StripeWebhook-lite] No bidId in metadata for payment_failed.");
                    }
                    else
                    {
                        var payment = await _paymentRepository.GetByBidIdAsync(bidId);
                        if (payment != null)
                        {
                            payment.Status = PaymentStatus.Failed;
                            payment.StatusReason = objEl.TryGetProperty("last_payment_error", out var lpe) && lpe.TryGetProperty("message", out var msg) ? msg.GetString() : "Payment failed";
                            payment.UpdatedAt = DateTime.UtcNow;
                            await _paymentRepository.UpdateAsync(payment);
                        }
                    }
                    _db.WebhookEventLogs.Add(new WebhookEventLog { EventId = id, ReceivedAt = DateTime.UtcNow });
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("[StripeWebhook-lite] Event {EventId} processed successfully.", id);
                    return true;
                }

                _logger.LogInformation("[StripeWebhook-lite] Unhandled event type: {EventType}", type);
                _db.WebhookEventLogs.Add(new WebhookEventLog { EventId = id, ReceivedAt = DateTime.UtcNow });
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StripeWebhook-lite] Failed to process event in dev-lite mode.");
                return false;
            }
        }
        private async Task<(string? currency, string? paymentIntentId, string? chargeId, string? receiptUrl, decimal? amountCaptured, decimal? processorFee, decimal? netAmount)> FetchStripePaymentDetailsAsync(string? paymentIntentId, string? fallbackCurrency)
        {
            string? currency = fallbackCurrency;
            string? chargeId = null;
            string? receiptUrl = null;
            decimal? amountCaptured = null;
            decimal? processorFee = null;
            decimal? netAmount = null;

            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                var piService = new PaymentIntentService();
                var pi = await piService.GetAsync(paymentIntentId);
                amountCaptured = (pi?.AmountReceived ?? 0) / 100m;
                currency ??= pi?.Currency;
                chargeId = pi?.LatestChargeId;
                if (!string.IsNullOrEmpty(chargeId))
                {
                    var chargeService = new ChargeService();
                    var ch = await chargeService.GetAsync(chargeId);
                    receiptUrl = ch?.ReceiptUrl;
                    currency ??= ch?.Currency;

                    var btService = new BalanceTransactionService();
                    if (!string.IsNullOrEmpty(ch?.BalanceTransactionId))
                    {
                        var bt = await btService.GetAsync(ch.BalanceTransactionId);
                        processorFee = (bt?.Fee ?? 0) / 100m;
                        netAmount = (bt?.Net ?? 0) / 100m;
                    }
                }
            }

            return (currency, paymentIntentId, chargeId, receiptUrl, amountCaptured, processorFee, netAmount);
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
