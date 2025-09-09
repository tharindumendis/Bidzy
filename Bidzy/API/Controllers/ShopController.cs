using Bidzy.API.DTOs;
using Bidzy.Application.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    public class ShopController : ControllerBase
    {
        private readonly IAuctionRepository auctionRepository;

        public ShopController(IAuctionRepository auctionRepository)
        {
            this.auctionRepository = auctionRepository;
        }
        [Authorize]
        [HttpGet("auctions")]
        public async Task<IActionResult> GetAllShopAuctionDetails()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var auctions = await auctionRepository.GetAllShopAuctionDetailsAsync(Guid.Parse(userId));
            if (auctions == null || !auctions.Any())
            {
                return NotFound("No auctions found for this shop");
            }
            return Ok(auctions.Select(a => a.ToshopAuctionDto()));
        }
    }
}
