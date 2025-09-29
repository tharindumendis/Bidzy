using Bidzy.Application.Services.Payments;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Bidzy.API.Controllers
{
    [ApiController]
    [Route("webhooks/stripe")] // Stripe webhook endpoint
    public class StripeWebhookController : ControllerBase
    {
        private readonly IStripePaymentService _stripePaymentService;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(IStripePaymentService stripePaymentService, ILogger<StripeWebhookController> logger)
        {
            _stripePaymentService = stripePaymentService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            using var reader = new StreamReader(HttpContext.Request.Body);
            var json = await reader.ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();

            _logger.LogDebug("Stripe webhook received. Path={Path} PayloadLength={Length}", Request.Path, json?.Length ?? 0);

            try
            {
                await _stripePaymentService.HandleWebhookAsync(json, signature);
                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe webhook rejected: {Message}", ex.Message);
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error processing Stripe webhook.");
                return StatusCode(500);
            }
        }
    }
}

