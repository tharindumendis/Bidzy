using Bidzy.Domain.Entities;
using Bidzy.Domain.Enum;
using Bidzy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.API.Controllers.Delivery
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeliveryController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public DeliveryController(ApplicationDbContext db) { _db = db; }

        [HttpGet("auction/{auctionId:guid}")]
        public async Task<IActionResult> GetByAuction([FromRoute] Guid auctionId)
        {
            var d = await _db.Deliveries.FirstOrDefaultAsync(x => x.AuctionId == auctionId);
            if (d == null) return NotFound();
            return Ok(new { d.Id, d.AuctionId, d.Status, d.ShippedAt, d.ConfirmedAt });
        }

        [HttpPost("mark-shipped")]
        public async Task<IActionResult> MarkShipped([FromQuery] Guid auctionId)
        {
            var delivery = await _db.Deliveries.FirstOrDefaultAsync(x => x.AuctionId == auctionId);
            if (delivery == null)
            {
                delivery = new Domain.Entities.Delivery { Id = Guid.NewGuid(), AuctionId = auctionId, Status = DeliveryStatus.Shipped, ShippedAt = DateTime.UtcNow };
                _db.Deliveries.Add(delivery);
            }
            else
            {
                delivery.Status = DeliveryStatus.Shipped;
                delivery.ShippedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
            return Ok(new { delivery.Id, delivery.Status, delivery.ShippedAt });
        }

        [HttpPut("confirm")]
        public async Task<IActionResult> Confirm([FromQuery] Guid auctionId)
        {
            var delivery = await _db.Deliveries.FirstOrDefaultAsync(x => x.AuctionId == auctionId);
            if (delivery == null) return NotFound();
            delivery.Status = DeliveryStatus.Confirmed;
            delivery.ConfirmedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { delivery.Id, delivery.Status, delivery.ConfirmedAt });
        }
    }
}
