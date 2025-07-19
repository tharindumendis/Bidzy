using Bidzy.API.Dto;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public BidController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult GetAllBids()
        {
            var bids = dbContext.Bids
                .Include(a => a.Auction)
                    .ThenInclude(p => p.Product)
                .Include(a => a.Auction)
                    .ThenInclude(w => w.Winner)
                .Include(b => b.Bidder)
                .ToList();
            return Ok(bids);
        }

        [HttpPost]
        public IActionResult AddBid(BidAddDto dto)
        {
            var auction = dbContext.Auctions.Find(dto.AuctionId);
            if (auction == null)
            {
                return BadRequest("Auction Id is invalid");
            }

            var bidder = dbContext.Users.Find(dto.BidderId);
            if (bidder == null)
            {
                return BadRequest("Bidder Id is invalid");
            }

            if (dto.Amount < auction.MinimumBid)
            {
                return BadRequest("Bid amount is less than minimum bid.");
            }

            var bidEntity = new Bid
            {
                AuctionId = dto.AuctionId,
                BidderId = dto.BidderId,
                Amount = dto.Amount,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                dbContext.Bids.Add(bidEntity);
                dbContext.SaveChanges();
                return Ok(bidEntity);
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
        public IActionResult UpdateBid(Guid id, BidAddDto dto)
        {
            var bid = dbContext.Bids.Find(id);
            if (bid == null)
            {
                return NotFound();
            }

            var auction = dbContext.Auctions.Find(dto.AuctionId);
            if (auction == null)
            {
                return BadRequest("Auction Id is invalid");
            }

            var bidder = dbContext.Users.Find(dto.BidderId);
            if (bidder == null)
            {
                return BadRequest("Bidder Id is invalid");
            }

            if (dto.Amount < auction.MinimumBid)
            {
                return BadRequest("Bid amount is less than minimum bid.");
            }

            bid.AuctionId = dto.AuctionId;
            bid.BidderId = dto.BidderId;
            bid.Amount = dto.Amount;
            bid.Timestamp = DateTime.UtcNow;

            try
            {
                dbContext.SaveChanges();
                return Ok(bid);
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
        public IActionResult DeleteBid(Guid id)
        {
            var bid = dbContext.Bids.Find(id);
            if (bid == null)
            {
                return NotFound();
            }

            dbContext.Bids.Remove(bid);
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
