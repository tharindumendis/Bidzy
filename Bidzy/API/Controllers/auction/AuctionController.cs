using System.Collections.Generic;
using System.Security.Claims;
using Bidzy.API.DTOs;
using Bidzy.API.DTOs.auction;
using Bidzy.Application.Mappers;
using Bidzy.Application.Repository.Auction;
using Bidzy.Application.Repository.Search;
using Bidzy.Application.Services.AuctionEngine;
using Bidzy.Domain.Entities;
using Bidzy.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit.Cryptography;

namespace Bidzy.API.Controllers.Auction
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var auctions = await auctionRepository.GetSuggestedAuctionsWithMaxBidAsync(Guid.Parse(userId));
            return Ok(auctions.Select(x => x.ToReadDto()));
        }
        [HttpGet("guess")]
        public async Task<IActionResult> GetAllGuessAuctions()
        {
            var auctions = await auctionRepository.GetAllActiveOrScheduledAuctionsWithMaxBidAsync();
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

        [Authorize(Roles = "Admin,Seller")]
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
        [Authorize(Roles = "Admin")]
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

        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAuctionsByUserId([FromRoute] Guid userId)
        {
            var auctions  = await auctionRepository.GetAuctionsByUserIdAsync(userId);
            return Ok(auctions.Select(x=> x.ToReadDto()));
        }
    }
}
