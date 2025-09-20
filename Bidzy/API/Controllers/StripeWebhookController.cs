using Bidzy.Application.Services.Payments;
using Microsoft.AspNetCore.Mvc;

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
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            _logger.LogInformation("Received webhook from IP: {RemoteIpAddress}", remoteIpAddress);

            using var reader = new StreamReader(HttpContext.Request.Body);
            var json = await reader.ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();

            try
            {
                await _stripePaymentService.HandleWebhookAsync(json, signature);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}

