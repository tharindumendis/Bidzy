using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IAppReviewRepository
    {
        Task<IEnumerable<AppReview>> GetAllAsync();
        Task<AppReview?> GetByIdAsync(Guid id);
        Task AddAsync(AppReview review);
        Task UpdateAsync(AppReview review);
        Task DeleteAsync(AppReview review);
    }    
}
