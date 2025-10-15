using Bidzy.API.DTOs;
using Bidzy.API.DTOs.bidDtos;
using Bidzy.Application.Mappers;
using Bidzy.Application.Repository.Bid;
using Bidzy.Application.Services.Bid;
using Bidzy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bidzy.API.Controllers.Bid
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidController : ControllerBase
    {
        private readonly IBidRepository bidRepository;
        private readonly IBidService bidService;    

        public BidController(IBidRepository bidRepository, IBidService bidService)
        {
            this.bidRepository = bidRepository;
            this.bidService = bidService;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllBids()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            List<Domain.Entities.Bid> bids = await bidService.GetAllBidsByUser(Guid.Parse(userId));
            return Ok(bids.Select(x => x.ToReadDto()));
        }
        [Authorize]
        [HttpGet("myBids")]
        public async Task<IActionResult> GetMyBids([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var pagedBids = await bidService.GetPagedBidsByUserAsync(Guid.Parse(userId), page, pageSize);

            return Ok(new
            {
                currentPage = page,
                pageSize,
                totalCount = pagedBids.TotalCount,
                totalPages = (int)Math.Ceiling(pagedBids.TotalCount / (double)pageSize),
                items = pagedBids.Items.Select(x => x.ToReadDto())
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBidById([FromRoute] Guid id)
        {
            var bid = await bidRepository.GetBidByIdAsync(id);
            if (bid == null)
            {
                return NotFound("Bid not found.");
            }
            return Ok(bid.ToReadDto());
        }

        //[Authorize]
        //[HttpGet("user")]
        //public async Task<IActionResult> GetBidsByUserId()
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var bids = await bidRepository.GetBidsByUserIdAsync(Guid.Parse(userId));
        //    if (bids == null || !bids.Any())
        //    {
        //        return NotFound("No bids found for this user.");
        //    }
        //    return Ok(bids.Select(b=>b.ToReadDto()));
        //}

        [HttpGet("auction/{auctionId}")]
        public async Task<IActionResult> GetBiddersByAuctionId([FromRoute] Guid auctionId)
        {
            var bidder = await bidRepository.GetBiddersByAuctionIdAsync(auctionId);
            if (bidder == null)
            {
                return NotFound("Bid not found");
            }
            return Ok(bidder.Select(b => b.ToReadDto()));
        }

        [HttpPost]
        public async Task<IActionResult> CreateBid([FromBody] BidAddDto bidAddDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bidAddDto.BidderId = Guid.Parse(userId);
            Domain.Entities.Bid entity = bidAddDto.ToEntity();
            Domain.Entities.Bid bid = await bidService.PlaceBid(entity);
            if (bid == null) return BadRequest();
            return Ok(bid.ToReadDto());
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateBid([FromRoute] Guid id, [FromBody] BidUpdateDto bidUpdateDto)
        {
            var bid = await bidRepository.GetBidByIdAsync(id);
            if (bid == null)
            {
                return NotFound("Bid not found.");
            }
            bid.UpdateEntity(bidUpdateDto);
            var updatedBid = await bidRepository.UpdateBidAsync(bid);
            return Ok(updatedBid);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteBidAsync([FromRoute] Guid id)
        {
            var bid = await bidRepository.GetBidByIdAsync(id);
            if (bid == null)
            {
                return NotFound("Bid not found.");
            }
            return NoContent();
        }

        [HttpGet("myActivity")]
        public async Task<IActionResult> GetBidderActivity()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId == null) return Unauthorized();
            var activity = await bidService.GetBidderActivityAsync(Guid.Parse(userId));
            return Ok(activity);
        }
    }
}
