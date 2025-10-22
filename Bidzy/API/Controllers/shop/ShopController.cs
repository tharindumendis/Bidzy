using Bidzy.API.DTOs;
using Bidzy.API.DTOs.auction;
using Bidzy.Application.Mappers;
using Bidzy.Application.Repository.Auction;
using Bidzy.Application.Repository.Product;
using Bidzy.Application.Services.AuctionEngine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bidzy.API.Controllers.Shop
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Seller")]
    public class ShopController : ControllerBase
    {
        private readonly IAuctionRepository auctionRepository;
        private readonly IAuctionEngine auctionEngine;
        private readonly IProductRepository productRepository;

        public ShopController(IAuctionRepository auctionRepository, IAuctionEngine auctionEngine, IProductRepository productRepository)
        {
            this.auctionRepository = auctionRepository;
            this.auctionEngine = auctionEngine;
            this.productRepository = productRepository;
        }
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
        [HttpGet("products")]
        public async Task<IActionResult> GetAllShopProductDetails()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var products = await productRepository.GetProductsByUserIdAsync(Guid.Parse(userId));
            if (products == null || !products.Any())
            {
                return NotFound("No products found for this shop");
            }
            return Ok(products.Select(a => a.ToReadDto()));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromForm] AuctionAddDto auctionAddDto)
        {
            DateTime rowStartTime = auctionAddDto.StartTime;
            DateTime rowEndTime = auctionAddDto.EndTime;
            return Ok(await auctionEngine.CreateAuctionAsync(auctionAddDto));
        }
    }
}
