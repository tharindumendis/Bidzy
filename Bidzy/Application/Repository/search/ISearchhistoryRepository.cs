using Bidzy.Domain.Entities;

namespace Bidzy.Application.Repository.Search
{
    public interface ISearchhistoryRepository
    {
        Task SaveSearchAsync(string query, Guid userId);
        Task<List<SearchHistory>> GetSearchHistoryByUserIdAsync(Guid userId);
        Task ClearSearchHistoryAsync(Guid userId);
        Task DeleteSearchHistoryByIdAsync(Guid historyId, Guid userId);
    }
}
