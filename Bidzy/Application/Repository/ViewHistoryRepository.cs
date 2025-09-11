using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Repository
{
    public class ViewHistoryRepository : IViewHistoryRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ViewHistoryRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async void SaveViewAsync(Guid auctionId, Guid userId)
        {
            var user = await dbContext.Users.FindAsync(userId);
            var auction = await dbContext.Auctions.FindAsync(auctionId);

            if (user == null || auction == null)
            {
                throw new InvalidOperationException("User or Auction not found.");
            }

            var viewHistory = new ViewHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = user,
                AuctionId = auctionId,
                Auction = auction,
                Timestamp = DateTime.UtcNow
            };

            dbContext.ViewHistories.Add(viewHistory);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<ViewHistory>> GetViewHistoryByUserIdAsync(Guid userId)
        {
            return await dbContext.ViewHistories
                .Where(vh => vh.UserId == userId)
                .OrderByDescending(vh => vh.Timestamp)
                .ToListAsync();
        }

        public async Task ClearViewHistoryAsync(Guid userId)
        {
            var histories = dbContext.ViewHistories
                .Where(vh => vh.UserId == userId);
            dbContext.ViewHistories.RemoveRange(histories);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteViewHistoryByIdAsync(Guid historyId, Guid userId)
        {
            var history = await dbContext.ViewHistories
                .FirstOrDefaultAsync(vh => vh.Id == historyId && vh.UserId == userId);
            if (history != null)
            {
                dbContext.ViewHistories.Remove(history);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
