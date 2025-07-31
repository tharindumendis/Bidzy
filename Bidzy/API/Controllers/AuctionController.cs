using Bidzy.API.DTOs;
using Bidzy.API.DTOs.auctionDtos;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionRepository auctionRepository;

        public AuctionController(IAuctionRepository auctionRepository)
        {
            this.auctionRepository = auctionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAuctions()
        {
            var auctions = await auctionRepository.GetAllAuctionsAsync();
            return Ok(auctions.Select(x => x.ToReadDto()));
        }

        [HttpGet("status/{statusId}")]
        public async Task<IActionResult> GetAllAuctionsByStatus([FromRoute] AuctionStatus statusId)
        {
            var auctions = await auctionRepository.GetAllAuctionsByStatusAsync(statusId);
            if (auctions == null || !auctions.Any())
            {
                return NotFound("No auctions found with the specified status");
            }
            return Ok(auctions.Select(x => x.ToReadDto()));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAuctionByIdAsync([FromRoute] Guid id)
        {
            var auctions = await auctionRepository.GetAuctionByIdAsync(id);
            if (auctions == null)
            {
                return NotFound("Auction not found");
            }
            return Ok(auctions.ToReadDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromBody] AuctionAddDto auctionAddDto)
        {
            var entity = auctionAddDto.ToEntity();
            var auction = await auctionRepository.AddAuctionAsync(entity);
            return Ok(auction.ToReadDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuction([FromRoute] Guid id, [FromBody] AuctionUpdateDto auctionUpdateDto)
        {
            var auction = await auctionRepository.GetAuctionByIdAsync(id);
            if(auction == null)
            {
                return NotFound("Auction not found");
            }
            auction.UpdateEntity(auctionUpdateDto);
            var updatedAuction = await auctionRepository.UpdateAuctionAsync(auction);
            return Ok(updatedAuction);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuction([FromRoute] Guid id)
        {
            var auction = await auctionRepository.DeleteAuctionAsync(id);
            if(auction == null)
            {
                return NotFound("Auction not found");
            }
            return NoContent();
        }



    }
}
