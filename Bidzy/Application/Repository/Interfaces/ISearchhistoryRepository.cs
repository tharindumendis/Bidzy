using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface ISearchhistoryRepository
    {
        void SaveSearchAsync(string query, Guid userId);
        Task<List<SearchHistory>> GetSearchHistoryByUserIdAsync(Guid userId);
        Task ClearSearchHistoryAsync(Guid userId);
        Task DeleteSearchHistoryByIdAsync(Guid historyId, Guid userId);
    }
}
