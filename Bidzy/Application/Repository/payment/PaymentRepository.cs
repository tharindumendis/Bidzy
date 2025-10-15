using Bidzy.Domain.Entities;
using Bidzy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Repository.Payment
{
    public class PaymentRepository(ApplicationDbContext dbContext) : IPaymentRepository
    {
        private readonly ApplicationDbContext _db = dbContext;

        public async Task<Domain.Entities.Payment?> GetByIdAsync(Guid id)
        {
            return await _db.Payments.FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Domain.Entities.Payment?> GetByBidIdAsync(Guid bidId)
        {
            return await _db.Payments.FirstOrDefaultAsync(p => p.BidId == bidId);
        }

        public async Task<Domain.Entities.Payment?> GetByChargeIdAsync(string chargeId)
        {
            if (string.IsNullOrWhiteSpace(chargeId)) return null;
            return await _db.Payments.FirstOrDefaultAsync(p => p.ChargeId == chargeId);
        }

        public async Task AddAsync(Domain.Entities.Payment payment)
        {
            await _db.Payments.AddAsync(payment);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Domain.Entities.Payment payment)
        {
            _db.Payments.Update(payment);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Payment>> GetByUserAsBuyerAsync(Guid userId)
        {
            return await _db.Payments
                .Where(p => _db.Bids.Any(b => b.Id == p.BidId && b.BidderId == userId))
                .OrderByDescending(p => p.PaidAt ?? p.CreatedAt)
                .ToListAsync();
        }
        public async Task<IEnumerable<Domain.Entities.Payment>> GetByUserAsSellerAsync(Guid userId)
        {
            var query = from p in _db.Payments
                        join b in _db.Bids on p.BidId equals b.Id
                        join a in _db.Auctions on b.AuctionId equals a.Id
                        where a.Product.SellerId == userId
                        orderby (p.PaidAt ?? p.CreatedAt) descending
                        select p;
            return await query.ToListAsync();
        }
        public async Task<IEnumerable<Domain.Entities.Payment>> ListRecentAsync(int take = 25)
        {
            return await _db.Payments
                .OrderByDescending(p => p.PaidAt ?? p.CreatedAt)
                .Take(Math.Clamp(take, 1, 200))
                .ToListAsync();
        }
    }
}
