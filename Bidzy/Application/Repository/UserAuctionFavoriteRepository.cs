using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Repository
{
    public class UserAuctionFavoriteRepository : IUserAuctionFavoriteRepository
    {
        private readonly ApplicationDbContext dbContext;

        public UserAuctionFavoriteRepository (ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<bool> ExistingAsync(Guid userId, Guid auctionId)
        {
            return await dbContext.UserAuctionFavorite
                .AnyAsync(x => x.userId == userId && x.auctionId == auctionId);
        }

        public async Task AddAsync (UserAuctionFavorite userAuctionFavorite)
        {
            dbContext.UserAuctionFavorite.Add(userAuctionFavorite);
            await dbContext.SaveChangesAsync();
        }

        public async Task RemoveAsync (Guid userId, Guid auctionId)
        {
            var userAuctionFavorite = await dbContext.UserAuctionFavorite.FindAsync(userId, auctionId);
            if(userAuctionFavorite != null)
            {
                dbContext.UserAuctionFavorite.Remove(userAuctionFavorite);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<UserAuctionFavorite>> GetFavoritesByUserAsync(Guid userId)
        {
            return await dbContext.UserAuctionFavorite
                .Where(x => x.userId == userId)
                .Include(x => x.auction)
                .ThenInclude(x => x.Product)
                .ToListAsync();
        }

        public async Task<UserAuctionFavorite?> GetByIdAsync(Guid userId, Guid auctionId)
        {
            return await dbContext.UserAuctionFavorite
                .Include(x => x.auction)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.userId == userId && x.auctionId == auctionId);
        }
    }
}
