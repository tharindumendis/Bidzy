using Bidzy.API.Dto;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> GetAllAuction()
        {
            var auctions = await auctionRepository.GetAllAuctionsAsync();
            return Ok(auctions);
        }

        [HttpPost]
        public async Task<IActionResult> AddAuction(AuctionAddDto dto)
        {
            var auctionEntity = await auctionRepository.AddAuctionAsync(dto);
            if (auctionEntity == null)
            {
                return BadRequest("Product Id is invalid or database error occurred.");
            }
            return Ok(auctionEntity);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAuction(Guid id, Auction dto)
        {
            var result = await auctionRepository.UpdateAuctionAsync(id, dto);
            if (!result)
            {
                return BadRequest("Auction or Product Id is invalid or database error occurred.");
            }
            return Ok(dto);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAuction(Guid id)
        {
            var result = await auctionRepository.DeleteAuctionAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
