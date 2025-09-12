using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IAppReviewRepository
    {
        Task<IEnumerable<AppReview>> GetAll();
        Task<AppReview?> GetById(Guid id);
        Task Add(AppReview review);
        Task Update(AppReview review);
        Task Delete(AppReview review);
    }    
}
