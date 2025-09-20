using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Repository
{
    public class PaymentRepository(ApplicationDbContext dbContext) : IPaymentRepository
    {
        private readonly ApplicationDbContext _db = dbContext;

        public async Task<Payment?> GetByIdAsync(Guid id)
        {
            return await _db.Payments.FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Payment?> GetByBidIdAsync(Guid bidId)
        {
            return await _db.Payments.FirstOrDefaultAsync(p => p.BidId == bidId);
        }

        public async Task<Payment?> GetByChargeIdAsync(string chargeId)
        {
            if (string.IsNullOrWhiteSpace(chargeId)) return null;
            return await _db.Payments.FirstOrDefaultAsync(p => p.ChargeId == chargeId);
        }

        public async Task AddAsync(Payment payment)
        {
            await _db.Payments.AddAsync(payment);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            _db.Payments.Update(payment);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Payment>> GetByUserAsBuyerAsync(Guid userId)
        {
            return await (from payment in _db.Payments
                          join bid in _db.Bids on payment.BidId equals bid.Id
                          where bid.BidderId == userId
                          orderby (payment.PaidAt ?? payment.CreatedAt) descending
                          select payment)
                .ToListAsync();
        }
        public async Task<IEnumerable<Payment>> GetByUserAsSellerAsync(Guid userId)
        {
            return await (from payment in _db.Payments
                          join bid in _db.Bids on payment.BidId equals bid.Id
                          join auction in _db.Auctions on bid.AuctionId equals auction.Id
                          join product in _db.Products on auction.ProductId equals product.Id
                          where product.SellerId == userId
                          orderby (payment.PaidAt ?? payment.CreatedAt) descending
                          select payment)
                .ToListAsync();
        }
        public async Task<IEnumerable<Payment>> ListRecentAsync(int take = 25)
        {
            return await _db.Payments
                .OrderByDescending(p => p.PaidAt ?? p.CreatedAt)
                .Take(Math.Clamp(take, 1, 200))
                .ToListAsync();
        }
    }
}
