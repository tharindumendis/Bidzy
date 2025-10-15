using Bidzy.Domain.Entities;

namespace Bidzy.Application.Repository.Payment
{
    public interface IPaymentRepository
    {
        Task<Domain.Entities.Payment?> GetByIdAsync(Guid id);
        Task<Domain.Entities.Payment?> GetByBidIdAsync(Guid bidId);
        Task<Domain.Entities.Payment?> GetByChargeIdAsync(string chargeId);
        Task<IEnumerable<Domain.Entities.Payment>> GetByUserAsBuyerAsync(Guid userId);
        Task<IEnumerable<Domain.Entities.Payment>> GetByUserAsSellerAsync(Guid userId);
        Task<IEnumerable<Domain.Entities.Payment>> ListRecentAsync(int take = 25);
        Task AddAsync(Domain.Entities.Payment payment);
        Task UpdateAsync(Domain.Entities.Payment payment);
    }
}
