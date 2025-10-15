using Bidzy.Domain.Entities;

namespace Bidzy.Application.Repository.view
{
    public interface IViewHistoryRepository
    {
        void SaveViewAsync(Guid auctionId, Guid userId);
        Task<List<ViewHistory>> GetViewHistoryByUserIdAsync(Guid userId);
        Task ClearViewHistoryAsync(Guid userId);
        Task DeleteViewHistoryByIdAsync(Guid historyId, Guid userId);
    }
}
