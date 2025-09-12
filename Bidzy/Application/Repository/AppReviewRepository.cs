using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Microsoft.EntityFrameworkCore;


namespace Bidzy.Application.Repository
{
    public class AppReviewRepository : IAppReviewRepository
    {
        private readonly ApplicationDbContext  _context;

        public AppReviewRepository(ApplicationDbContext context)
        {
             _context = context;
        }

        public async Task<IEnumerable<AppReview>> GetAll() => await _context.AppReviews.OrderByDescending(r => r.CreatedAt);

        public async Task<AppReview?> GetById(Guid id) =>await _context.AppReviews.Find(id);

        public async void Add(AppReview review)
        {
        
            await  _context.AppReviews.Add(review);
            await _context.SaveChanges();
        }

        public async void Update(AppReview review)
        {
            await _context.AppReviews.Update(review);
            await _context.SaveChanges();
        }

        public async void Delete(AppReview review)
        {
            await _context.AppReviews.Remove(review);
            await _context.SaveChanges();
        }
    }
}
