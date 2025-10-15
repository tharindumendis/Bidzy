using Bidzy.API.DTOs.favoriteAuctionsDtos;
using Bidzy.Domain.Entities;

namespace Bidzy.Application.Mappers
{
    public static class UserAuctionFavoriteDtoMapper
    {
        public static userAuctionFavoriteReadDto ToReadDto (this UserAuctionFavorite entity)
        {
            return new userAuctionFavoriteReadDto
            {
                auctionId = entity.auctionId,
                productTitle = entity.auction?.Product?.Title ?? "Unknown",
                LikedAt = entity.likedAt
            };
        }

        public static UserAuctionFavorite ToEntity (this userAuctionFavoriteCreateDto addLikeDto , Guid userId)
        {
            return new UserAuctionFavorite
            {
                userId = userId,
                auctionId = addLikeDto.auctionId,
                likedAt = DateTime.UtcNow
            };
        }
    }
}
