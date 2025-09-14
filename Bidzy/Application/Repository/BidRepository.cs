using Bidzy.API.DTOs;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Repository
{
    public class BidRepository : IBidRepository
    {
        private readonly ApplicationDbContext dbContext;

        public BidRepository (ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Bid>> GetAllBidsAsync()
        {
            return await dbContext.Bids
                .Include(a => a.Auction)
                .Include(b => b.Bidder)
                .ToListAsync();
        }

        public async Task<Bid?> GetBidByIdAsync(Guid bidId)
        {
            return await dbContext.Bids
                .Include(a => a.Auction)
                    .ThenInclude(a => a.Product)
                .Include(b => b.Bidder)
                .FirstOrDefaultAsync(x => x.Id == bidId);
        }

        public async Task<List<Bid>> GetBiddersByAuctionIdAsync(Guid auctionId)
        {
            //TODO can optimize fetch auction separatly and combine
            return await dbContext.Bids
                .Where(b => b.AuctionId == auctionId)
                .Include(a => a.Auction)
                    .ThenInclude(b => b.Product)
                .Include(b => b.Bidder)
                .ToListAsync();
        }

        public async Task<List<Bid>> GetBidsByUserIdAsync(Guid userId)
        {
            return await dbContext.Bids
                .Where(b=> b.BidderId == userId)
                .Include(a => a.Auction)
                    .ThenInclude(b => b.Product)
                .Include(b => b.Bidder)
                .ToListAsync();
        }

        public async Task<Bid?> AddBidAsync(Bid bid)
        {
            dbContext.Bids.Add(bid);
            await dbContext.SaveChangesAsync();
            var newBid = await GetBidByIdAsync(bid.Id);
            return newBid;
        }

        public async Task<Bid?> UpdateBidAsync(Bid bid)
        {
            dbContext.Bids.Update(bid);
            await dbContext.SaveChangesAsync();
            return bid;
        }

        public async Task<Bid?> DeleteBidAsync(Guid bidId)
        {
            var bids = await dbContext.Bids.FirstOrDefaultAsync(x => x.Id == bidId);
            if (bids == null)
            {
                return null;
            }
            dbContext.Bids.Remove(bids);
            await dbContext.SaveChangesAsync();
            return bids;
        }
        public IQueryable<Bid> Query() => dbContext.Bids.AsQueryable();
        public Task<Bid?> GetWinningBidAsync(Guid auctionId, DateTime endTime)
        {
            return dbContext.Bids
                .Where(bid => bid.AuctionId == auctionId && bid.Timestamp <= endTime)
                .Include(a => a.Auction)
                    .ThenInclude(a => a.Product)
                .OrderByDescending(bid => bid.Amount)
                .ThenBy(bid => bid.Timestamp)
                .FirstOrDefaultAsync();
        }
        public async Task<PagedResult<Bid>> GetPagedBidsByUserAsync(Guid userId, int page, int pageSize)
        {
            var query = dbContext.Bids
                .Where(b => b.BidderId == userId)
                .Include(b => b.Auction)
                    .ThenInclude(a => a.Product)
                .OrderByDescending(b => b.Timestamp); // Sort by latest bid

            var totalCount = await query.CountAsync(); // Safe sequential call
            var pagedItems = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Bid>
            {
                TotalCount = totalCount,
                Items = pagedItems
            };
        }
    }
}
