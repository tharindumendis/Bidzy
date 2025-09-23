using System.Security.Claims;
using Bidzy.API.DTOs.paymentDtos;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services.Payments;
using Bidzy.Application.Settings;
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
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IAuctionRepository auctionRepository,
            IBidRepository bidRepository,
            IPaymentRepository paymentRepository,
            IStripePaymentService stripePaymentService,
            IOptions<StripeSettings> stripeSettings,
            ILogger<PaymentController> logger)
        {
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _paymentRepository = paymentRepository;
            _stripePaymentService = stripePaymentService;
            _stripeSettings = stripeSettings;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("checkout-session")] // creates a Checkout session for auction winner
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            if (request == null || request.AuctionId == Guid.Empty)
                return BadRequest("Invalid request");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            _logger.LogInformation("[Payment] Create checkout requested for auction {AuctionId} by user {UserId}", request.AuctionId, userId);

            var auction = await _auctionRepository.GetAuctionByIdAsync(request.AuctionId);
            if (auction == null) return NotFound("Auction not found.") ;
            if (auction.WinningBidId == null) return BadRequest("Auction has no winner yet.");

            var winningBid = await _bidRepository.GetBidByIdAsync(auction.WinningBidId.Value);
            if (winningBid == null) return BadRequest("Winning bid not found.");
            if (winningBid.BidderId.ToString() != userId)
            {
                _logger.LogWarning("[Payment] User {UserId} attempted to pay for bid {BidId} not owned by them", userId, auction.WinningBidId);
                return Forbid();
            }

            // prevent duplicate payments
            var existing = await _paymentRepository.GetByBidIdAsync(winningBid.Id);
            if (existing != null && existing.Status == Domain.Enum.PaymentStatus.Completed)
            {
                _logger.LogInformation("[Payment] Payment already completed for bid {BidId}", winningBid.Id);
                return BadRequest("Payment already completed for this auction.");
            }

            if (string.IsNullOrWhiteSpace(request.SuccessUrl) || string.IsNullOrWhiteSpace(request.CancelUrl))
            {
                return BadRequest("SuccessUrl and CancelUrl are required for frontend integration.");
            }
            else
            {
                _logger.LogInformation("[Payment] Using successUrl={SuccessUrl} cancelUrl={CancelUrl}", request.SuccessUrl, request.CancelUrl);
            }

            // Create Pending Payment if none exists
            var commission = Math.Round(winningBid.Amount * _stripeSettings.Value.CommissionRate, 2, MidpointRounding.AwayFromZero);
            var totalAmount = winningBid.Amount + commission;

            if (existing == null)
            {
                var pendingPayment = new Domain.Enties.Payment
                {
                    Id = Guid.NewGuid(),
                    BidId = winningBid.Id,
                    UserId = winningBid.BidderId,
                    TotalAmount = totalAmount,
                    Commission = commission,
                    Currency = _stripeSettings.Value.Currency,
                    Status = Domain.Enum.PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await _paymentRepository.AddAsync(pendingPayment);
                _logger.LogInformation("[Payment] Pending payment created for bid {BidId} amount={Total} commission={Commission}", pendingPayment.BidId, pendingPayment.TotalAmount, pendingPayment.Commission);
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
            _logger.LogInformation("[Payment] Checkout session url generated for bid {BidId}", winningBid.Id);
            return Ok(new { url });
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPaymentById([FromRoute] Guid id)
        {
            var p = await _paymentRepository.GetByIdAsync(id);
            if (p == null) return NotFound();
            var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimsUserId)) return Unauthorized();
            if (p.UserId.ToString() != claimsUserId && !User.IsInRole("Admin")) return Forbid();
            return Ok(ToDto(p));
        }

        [Authorize]
        [HttpGet("bid/{bidId:guid}")]
        public async Task<IActionResult> GetByBid([FromRoute] Guid bidId)
        {
            var p = await _paymentRepository.GetByBidIdAsync(bidId);
            if (p == null) return NotFound();
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            if (!User.IsInRole("Admin"))
            {
                var isBuyer = p.UserId == userId;
                var isSeller = false;

                if (!isBuyer)
                {
                    var bid = await _bidRepository.GetBidByIdAsync(p.BidId);
                    if (bid != null)
                    {
                        var auction = await _auctionRepository.GetAuctionByIdAsync(bid.AuctionId);
                        if (auction?.Product != null)
                        {
                            isSeller = auction.Product.SellerId == userId;
                        }
                    }
                }

                if (!isBuyer && !isSeller)
                {
                    return Forbid();
                }
            }

            return Ok(ToDto(p));
        }

        [Authorize]
        [HttpGet("auction/{auctionId:guid}")]
        public async Task<IActionResult> GetByAuction([FromRoute] Guid auctionId)
        {
            var auction = await _auctionRepository.GetAuctionByIdAsync(auctionId);
            if (auction == null) return NotFound("Auction not found");
            if (auction.WinningBidId == null) return NotFound("Auction has no winning bid");

            var p = await _paymentRepository.GetByBidIdAsync(auction.WinningBidId.Value);
            if (p == null) return NotFound();
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            if (!User.IsInRole("Admin"))
            {
                var isBuyer = p.UserId == userId;
                var isSeller = auction.Product?.SellerId == userId;

                if (!isBuyer && !isSeller)
                {
                    return Forbid();
                }
            }
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
