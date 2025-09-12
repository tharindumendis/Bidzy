using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bidzy.Domain.Enties;

namespace Bidzy.Application.Services
{
    public interface IAppReviewService
    {
        Task<IEnumerable<AppReview>> GetAllReviews();
        Task<AppReview?> GetReview(Guid id);
        Task AddReview(AppReview review);
        Task UpdateReview(AppReview review);
        Task DeleteReview(AppReview review);
    }
}