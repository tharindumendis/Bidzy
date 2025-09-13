using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Microsoft.EntityFrameworkCore; // for async EF methods

namespace Bidzy.Infrastructure.Repository
{
    public class AppReviewRepository : IAppReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public AppReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppReview>> GetAllAsync() 
        => await _context.AppReviews.OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<AppReview?> GetByIdAsync(Guid id) 
        => await _context.AppReviews.FindAsync(id);

    public async Task AddAsync(AppReview review)
    {
        await _context.AppReviews.AddAsync(review);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AppReview review)
    {
        _context.AppReviews.Update(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AppReview review)
    {
        _context.AppReviews.Remove(review);
        await _context.SaveChangesAsync();
    }
    }
}
