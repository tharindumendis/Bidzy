using Bidzy.API.DTOs;
using Bidzy.API.DTOs.Admin;
using Bidzy.API.DTOs.userDtos;
using Bidzy.Application.Mappers;
using Bidzy.Application.Services.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService adminService;
        public AdminController(IAdminService adminService)
        {
            this.adminService = adminService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("users/{userId}/activate")]
        public async Task<IActionResult> ActivateUser(Guid userId)
        {
            var result = await adminService.ActivateUserAsync(userId);
            if (!result)
                return NotFound(new { Message = "User not found or could not be activated." });
            return Ok(new { Message = "User activated successfully." });
        }

        [HttpPut("users/{userId}/deactivate")]
        public async Task<IActionResult> DeactivateUser(Guid userId)
        {
            var result = await adminService.DeactivateUserAsync(userId);
            if (!result)
                return NotFound(new { Message = "User not found or could not be deactivated." });
            return Ok(new { Message = "User deactivated successfully." });
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await adminService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpPut("products/{productId}/activate")]
        public async Task<IActionResult> ActivateProduct(Guid productId)
        {
            var product = await adminService.ActivateProductAsync(productId);
            if (product == null)
                return NotFound(new { Message = "Product not found or could not be activated." });
            return Ok(new { Message = "Product activated successfully.", Product = product });
        }

        [HttpPut("products/{productId}/deactivate")]
        public async Task<IActionResult> DeactivateProduct(Guid productId)
        {
            var product = await adminService.DeactivateProductAsync(productId);
            if (product == null)
                return NotFound(new { Message = "Product not found or could not be deactivated." });
            return Ok(new { Message = "Product deactivated successfully.", Product = product });
        }

        [HttpGet("auctions")]
        public async Task<IActionResult> GetAllAuctions()
        {
            var auctions = await adminService.GetAllAuctionsAsync();
            return Ok(auctions.Select(x => x.ToReadDto()));
        }

        [HttpPut("auctions/{auctionId}/cancel")]
        public async Task<IActionResult> CancelAuction(Guid auctionId)
        {
            var auction = await adminService.CancelAuctionAsync(auctionId);
            if (auction == null)
                return NotFound(new { Message = "Auction not found or could not be cancelled." });
            return Ok(new { Message = "Auction cancelled successfully.", Auction = auction });
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAllAnalytics()
        {
            return Ok(await adminService.GetSiteAnalyticsAsync());
        }

        [HttpPost("addAdmin")]
        public async Task<IActionResult> AddAdmin([FromBody] AddAdminDto adminCreateDto)
        {
            var adminUser = await adminService.AddAdminUserAsync(adminCreateDto);
            return Ok(new { Message = "Admin user created successfully.", User = adminUser });
        }
    }
}
