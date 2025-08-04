using Bidzy.API.DTOs;
using Bidzy.API.DTOs.bidDtos;
using Bidzy.Application.Repository;
using Bidzy.Application.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidController : ControllerBase
    {
        private readonly IBidRepository bidRepository;

        public BidController(IBidRepository bidRepository)
        {
            this.bidRepository = bidRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBids()
        {
            var bids = await bidRepository.GetAllBidsAsync();
            return Ok(bids.Select(x => x.ToReadDto()));
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
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBidsByUserId([FromRoute] Guid userId)
        {
            var bids = await bidRepository.GetBidsByUserIdAsync(userId);
            if (bids == null || !bids.Any())
            {
                return NotFound("No bids found for this user.");
            }
            return Ok(bids.Select(b=>b.ToReadDto()));
        }

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
            var entity = bidAddDto.ToEntity();
            var bid = await bidRepository.AddBidAsync(entity);
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
    }
}
