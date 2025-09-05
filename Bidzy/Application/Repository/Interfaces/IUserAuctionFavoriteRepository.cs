using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IUserAuctionFavoriteRepository
    {
        Task<bool> ExistingAsync(Guid userId, Guid auctionId);
        Task AddAsync(UserAuctionFavorite entity);
        Task RemoveAsync (Guid userId, Guid auctionId);
        Task<IEnumerable<UserAuctionFavorite>> GetFavoritesByUserAsync(Guid userId);
        Task<IEnumerable<UserAuctionFavorite>> GetFavoritesByAuctionAsync(Guid auctionId);
        Task<UserAuctionFavorite?> GetByIdAsync(Guid userId , Guid auctionId);
    }
}
