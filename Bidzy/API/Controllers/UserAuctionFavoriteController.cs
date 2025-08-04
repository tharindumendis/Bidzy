using Bidzy.API.DTOs;
using Bidzy.API.DTOs.favoriteAuctionsDtos;
using Bidzy.Application.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuctionFavoriteController : ControllerBase
    {
        public readonly IUserAuctionFavoriteRepository userAuctionFavoriteRepository;

        public UserAuctionFavoriteController(IUserAuctionFavoriteRepository userAuctionFavoriteRepository)
        {
            this.userAuctionFavoriteRepository = userAuctionFavoriteRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] userAuctionFavoriteCreateDto favoriteCreateDto, [FromQuery] Guid userId)
        {
            var existingFavorite = await userAuctionFavoriteRepository.ExistingAsync(userId, favoriteCreateDto.auctionId);
            if (existingFavorite)
            {
                return BadRequest("This auction is already in your favorites.");
            }
            var entity = favoriteCreateDto.ToEntity(userId);
            await userAuctionFavoriteRepository.AddAsync(entity);

            return Ok("Auction added to favorites successfully.");

        }

        [HttpDelete]
        public async Task<IActionResult> RemoveFavorite([FromQuery] Guid userId, [FromQuery] Guid auctionId)
        {
            var existingFavorite = await userAuctionFavoriteRepository.ExistingAsync(userId, auctionId);
            if (!existingFavorite)
            {
                return NotFound("This auction is not in your favorites.");
            }
            await userAuctionFavoriteRepository.RemoveAsync(userId, auctionId);
            return Ok("Auction removed from favorites successfully.");
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserFavorites([FromRoute] Guid userId)
        {
            var favourites = await userAuctionFavoriteRepository.GetFavoritesByUserAsync(userId);
            var dtoList = favourites.Select(f=> f.ToReadDto()).ToList();
            return Ok(dtoList);
        }

    }
}
