using Bidzy.Data;
using Bidzy.Modles;
using Bidzy.Modles.Dto;
using Bidzy.Modles.Enties;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public AuctionController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult GetAllAuction()
        {
            var auctions = dbContext.Auctions.ToList();

            return Ok(auctions);
        }
        [HttpPost]
        public IActionResult AddAuction(AuctionAddDto dto)
        {
            var product = dbContext.products.Find(dto.ProductId);
            if (product == null)
            {
                return BadRequest("Product Id is invalid");
            }

            var auctionEntity = new Auction
            {
                ProductId = dto.ProductId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                MinimumBid = dto.MinimumBid,
                Status = AuctionStatus.Scheduled,
            };

            try
            {
                dbContext.Auctions.Add(auctionEntity);
                dbContext.SaveChanges();
                return Ok(auctionEntity);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPut("{id:guid}")]
        public IActionResult UpdateAuction(Guid id, Auction dto)
        {
            var auction = dbContext.Auctions.Find(id);
            if (auction == null)
            {
                return NotFound();
            }

            var product = dbContext.products.Find(dto.ProductId);
            if (product == null)
            {
                return BadRequest("Product Id is invalid");
            }

            auction.ProductId = dto.ProductId;
            auction.StartTime = dto.StartTime;
            auction.EndTime = dto.EndTime;
            auction.MinimumBid = dto.MinimumBid;
            auction.Status = dto.Status;
            auction.WinnerId = dto.WinnerId;

            try
            {
                dbContext.SaveChanges();
                return Ok(auction);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteAuction(Guid id)
        {
            var auction = dbContext.Auctions.Find(id);
            if (auction == null)
            {
                return NotFound();
            }

            dbContext.Auctions.Remove(auction);
            try
            {
                dbContext.SaveChanges();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

    }
}
