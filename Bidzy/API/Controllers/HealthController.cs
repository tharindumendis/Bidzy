using Microsoft.AspNetCore.Mvc;
using Bidzy.Data;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.API.Controllers
{
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public HealthController(ApplicationDbContext db) { _db = db; }

        [HttpGet("/healthz")]
        public IActionResult Health() => Ok(new { status = "ok", time = DateTime.UtcNow });

        [HttpGet("/webhookz")]
        public async Task<IActionResult> WebhookHealth()
        {
            var last = await _db.WebhookEventLogs.OrderByDescending(e => e.ReceivedAt).FirstOrDefaultAsync();
            var count = await _db.WebhookEventLogs.CountAsync();
            return Ok(new { count, last = last?.ReceivedAt });
        }
    }
}
