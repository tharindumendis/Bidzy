using Bidzy.Domain.Entities;
using Bidzy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;

namespace Bidzy.Application.Repository.Search
{
    public class SearchHistoryRepository : ISearchhistoryRepository
    {
        private readonly ApplicationDbContext dbContext;

        public SearchHistoryRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<SearchHistory>> GetSearchHistoryByUserIdAsync(Guid userId)
        {
            return await dbContext.SearchHistories
                .Where (sh => sh.UserId == userId)
                .OrderByDescending(sh => sh.Timestamp)
                .ToListAsync();
        }

        public async Task SaveSearchAsync(string query, Guid userId)
        {
            SearchHistory searchHistory = new SearchHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Query = query,
                Timestamp = DateTime.UtcNow
            };
            dbContext.SearchHistories.Add(searchHistory);
            await dbContext.SaveChangesAsync();
        }

        public async Task ClearSearchHistoryAsync(Guid userId)
        {
            var histories = dbContext.SearchHistories
                .Where(h => h.UserId == userId);
            dbContext.SearchHistories.RemoveRange(histories);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteSearchHistoryByIdAsync (Guid historyId , Guid userId)
        {
            var history = await dbContext.SearchHistories
                .FirstOrDefaultAsync(h => h.Id == historyId && h.UserId == userId);
            if (history != null)
            {
                dbContext.SearchHistories.Remove(history);
                await dbContext.SaveChangesAsync();
            }
        }

    }
}
