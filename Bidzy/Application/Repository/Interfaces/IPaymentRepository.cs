using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(Guid id);
        Task<Payment?> GetByBidIdAsync(Guid bidId);
        Task<IEnumerable<Payment>> GetByUserAsBuyerAsync(Guid userId);
        Task<IEnumerable<Payment>> GetByUserAsSellerAsync(Guid userId);
        Task<IEnumerable<Payment>> ListRecentAsync(int take = 25);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
    }
}
