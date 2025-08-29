using Bidzy.API.DTOs;
using Bidzy.API.DTOs.auctionDtos;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services.AuctionEngine;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit.Cryptography;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionRepository auctionRepository;
        private readonly IAuctionEngine _auctionEngine;
        private readonly ISearchhistoryRepository searchhistoryRepository;


        public AuctionController(IAuctionRepository auctionRepository, IAuctionEngine auctionEngine, ISearchhistoryRepository searchhistoryRepository)
        {
            this.auctionRepository = auctionRepository;
            _auctionEngine = auctionEngine;
            this.searchhistoryRepository = searchhistoryRepository;
        }

        [Authorize]
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
            
            return Ok(await _auctionEngine.CreateAuctionAsync(auctionAddDto));
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

        [HttpGet ("search")]
        public async Task<IActionResult> getSearchedAuctions([FromQuery] string query)
        {
            var auctions = await auctionRepository.GetAllAuctionsAsync();
            var filteredAuctions = auctions.Where(q =>
                q.Product.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
            searchhistoryRepository.SaveSearchAsync(query, Guid.Parse("f0029d2c-1863-4cfe-8ebc-adf7549b9ab7"));
            return Ok(filteredAuctions.Select(x => x.ToReadDto()));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAuctionsByUserId([FromRoute] Guid userId)
        {
            var auctions  = await auctionRepository.GetAuctionsByUserIdAsync(userId);
            return Ok(auctions.Select(x=> x.ToReadDto()));
        }
    }
}
