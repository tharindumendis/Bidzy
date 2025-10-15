using System.Security.Claims;
using Bidzy.API.DTOs.paymentDtos;
using Bidzy.Application.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers.Payments
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RefundController : ControllerBase
    {
        private readonly IStripePaymentService _stripePaymentService;
        public RefundController(IStripePaymentService stripePaymentService)
        {
            _stripePaymentService = stripePaymentService;
        }

        [HttpPost("{paymentId:guid}")]
        public async Task<IActionResult> CreateRefund([FromRoute] Guid paymentId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            try
            {
                var payment = await _stripePaymentService.CreateRefundAsync(paymentId, userId);
                var dto = new PaymentDto
                {
                    Id = payment.Id,
                    BidId = payment.BidId,
                    TotalAmount = payment.TotalAmount,
                    Commission = payment.Commission,
                    Currency = payment.Currency,
                    AmountCaptured = payment.AmountCaptured,
                    ProcessorFee = payment.ProcessorFee,
                    NetAmount = payment.NetAmount,
                    PaymentIntentId = payment.PaymentIntentId,
                    ChargeId = payment.ChargeId,
                    ReceiptUrl = payment.ReceiptUrl,
                    Status = payment.Status.ToString(),
                    PaidAt = payment.PaidAt,
                    CreatedAt = payment.CreatedAt,
                    UpdatedAt = payment.UpdatedAt,
                    RefundId = payment.RefundId,
                    RefundAmount = payment.RefundAmount,
                    RefundStatus = payment.RefundStatus,
                    RefundedAt = payment.RefundedAt
                };
                return Ok(dto);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing the refund.");
            }
        }
    }
}

