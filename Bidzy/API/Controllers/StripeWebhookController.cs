using Bidzy.Application.Services.Payments;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
            // Implement IP whitelisting
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            _logger.LogInformation("Received webhook from IP: {RemoteIpAddress}", remoteIpAddress);

            var stripeIpAddresses = new List<IPAddress>
            {
                IPAddress.Parse("3.18.12.63"),
                IPAddress.Parse("3.130.192.231"),
                IPAddress.Parse("13.235.14.237"),
                IPAddress.Parse("13.235.122.149"),
                IPAddress.Parse("18.211.135.69"),
                IPAddress.Parse("35.154.171.200"),
                IPAddress.Parse("52.15.183.38"),
                IPAddress.Parse("54.88.130.119"),
                IPAddress.Parse("54.88.130.237"),
                IPAddress.Parse("54.187.174.169"),
                IPAddress.Parse("54.187.205.235"),
                IPAddress.Parse("54.187.216.72")
            };

            if (remoteIpAddress == null || !stripeIpAddresses.Any(ip => ip.Equals(remoteIpAddress)))
            {
                _logger.LogWarning("Webhook received from unauthorized IP: {RemoteIpAddress}", remoteIpAddress);
                return BadRequest("Unauthorized IP Address");
            }

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

