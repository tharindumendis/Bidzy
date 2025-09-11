using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IViewHistoryRepository
    {
        void SaveViewAsync(Guid auctionId, Guid userId);
        Task<List<ViewHistory>> GetViewHistoryByUserIdAsync(Guid userId);
        Task ClearViewHistoryAsync(Guid userId);
        Task DeleteViewHistoryByIdAsync(Guid historyId, Guid userId);
    }
}
