using System.Security.Claims;
using Bidzy.API.DTOs.paymentDtos;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services.Payments;
using Bidzy.Application.Settings;
using Bidzy.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bidzy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IStripePaymentService _stripePaymentService;
        private readonly IOptions<StripeSettings> _stripeSettings;

        public PaymentController(
            IAuctionRepository auctionRepository,
            IBidRepository bidRepository,
            IPaymentRepository paymentRepository,
            IStripePaymentService stripePaymentService,
            IOptions<StripeSettings> stripeSettings)
        {
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _paymentRepository = paymentRepository;
            _stripePaymentService = stripePaymentService;
            _stripeSettings = stripeSettings;
        }

        [Authorize]
        [HttpPost("checkout-session")] // creates a Checkout session for auction winner
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            if (request == null || request.AuctionId == Guid.Empty)
                return BadRequest("Invalid request");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var auction = await _auctionRepository.GetAuctionByIdAsync(request.AuctionId);
            if (auction == null) return NotFound("Auction not found.") ;
            if (auction.WinningBidId == null) return BadRequest("Auction has no winner yet.");

            var winningBid = await _bidRepository.GetBidByIdAsync(auction.WinningBidId.Value);
            if (winningBid == null) return BadRequest("Winning bid not found.");
            if (winningBid.BidderId.ToString() != userId) return Forbid();

            // prevent duplicate payments
            var existing = await _paymentRepository.GetByBidIdAsync(winningBid.Id);
            if (existing != null && existing.Status == Domain.Enum.PaymentStatus.Completed)
                return BadRequest("Payment already completed for this auction.");

            if (string.IsNullOrWhiteSpace(request.SuccessUrl) || string.IsNullOrWhiteSpace(request.CancelUrl))
            {
                return BadRequest("SuccessUrl and CancelUrl are required for frontend integration.");
            }

            // Ensure there is a single pending payment record to reconcile against Stripe
            var commission = Math.Round(winningBid.Amount * _stripeSettings.Value.CommissionRate, 4, MidpointRounding.AwayFromZero);
            var totalAmount = winningBid.Amount + commission;
            var currency = _stripeSettings.Value.Currency;

            var paymentRecord = existing;
            if (paymentRecord == null)
            {
                paymentRecord = new Domain.Enties.Payment
                {
                    Id = Guid.NewGuid(),
                    BidId = winningBid.Id,
                    UserId = winningBid.BidderId,
                    TotalAmount = totalAmount,
                    Commission = commission,
                    Currency = currency,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await _paymentRepository.AddAsync(paymentRecord);
            }
            else
            {
                paymentRecord.TotalAmount = totalAmount;
                paymentRecord.Commission = commission;
                paymentRecord.Currency = currency;
                paymentRecord.Status = PaymentStatus.Pending;
                paymentRecord.StatusReason = null;
                paymentRecord.PaymentIntentId = null;
                paymentRecord.ChargeId = null;
                paymentRecord.AmountCaptured = null;
                paymentRecord.ProcessorFee = null;
                paymentRecord.NetAmount = null;
                paymentRecord.ReceiptUrl = null;
                paymentRecord.PaidAt = null;
                paymentRecord.RefundId = null;
                paymentRecord.RefundAmount = null;
                paymentRecord.RefundStatus = null;
                paymentRecord.RefundedAt = null;
                paymentRecord.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(paymentRecord);
            }

            string success = request.SuccessUrl;
            string cancel = request.CancelUrl;

            var url = await _stripePaymentService.CreateCheckoutSessionForWinningBidAsync(
                winningBid,
                _stripeSettings.Value.CommissionRate,
                _stripeSettings.Value.Currency,
                successUrl: success,
                cancelUrl: cancel
            );

            return Ok(new { url });
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPaymentById([FromRoute] Guid id)
        {
            var p = await _paymentRepository.GetByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(ToDto(p));
        }

        [Authorize]
        [HttpGet("bid/{bidId:guid}")]
        public async Task<IActionResult> GetByBid([FromRoute] Guid bidId)
        {
            var p = await _paymentRepository.GetByBidIdAsync(bidId);
            if (p == null) return NotFound();
            return Ok(ToDto(p));
        }

        [Authorize]
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUser([FromRoute] Guid userId, [FromQuery] string role = "buyer")
        {
            var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.Parse(claimsUserId) != userId) return Forbid("You can only view your own payments.");

            IEnumerable<Domain.Enties.Payment> list = role.Equals("seller", StringComparison.OrdinalIgnoreCase)
                ? await _paymentRepository.GetByUserAsSellerAsync(userId)
                : await _paymentRepository.GetByUserAsBuyerAsync(userId);
            return Ok(list.Select(ToDto));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("recent")]
        public async Task<IActionResult> Recent([FromQuery] int take = 25)
        {
            var list = await _paymentRepository.ListRecentAsync(take);
            return Ok(list.Select(ToDto));
        }

        private static PaymentDto ToDto(Domain.Enties.Payment p) => new PaymentDto
        {
            Id = p.Id,
            BidId = p.BidId,
            TotalAmount = p.TotalAmount,
            Commission = p.Commission,
            Currency = p.Currency,
            AmountCaptured = p.AmountCaptured,
            ProcessorFee = p.ProcessorFee,
            NetAmount = p.NetAmount,
            PaymentIntentId = p.PaymentIntentId,
            ChargeId = p.ChargeId,
            ReceiptUrl = p.ReceiptUrl,
            Status = p.Status.ToString(),
            PaidAt = p.PaidAt,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            RefundId = p.RefundId,
            RefundAmount = p.RefundAmount,
            RefundStatus = p.RefundStatus,
            RefundedAt = p.RefundedAt
        };
    }
}
