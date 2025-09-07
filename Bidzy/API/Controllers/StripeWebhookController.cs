using Bidzy.Application.Services.Payments;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers
{
    [ApiController]
    [Route("webhooks/stripe")] // Stripe webhook endpoint
    public class StripeWebhookController : ControllerBase
    {
        private readonly IStripePaymentService _stripePaymentService;

        public StripeWebhookController(IStripePaymentService stripePaymentService)
        {
            _stripePaymentService = stripePaymentService;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
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

