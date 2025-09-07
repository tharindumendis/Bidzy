using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Repository
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly ApplicationDbContext dbContext;

        public AuctionRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Auction>> GetAllAuctionsAsync()
        {
            return await dbContext.Auctions
                .Include(a => a.Product)
                    .ThenInclude(s => s.Seller)
                .Include(b => b.WinningBid)
                    .ThenInclude(a => a.Bidder)
                .ToListAsync();
        }

        public async Task<List<Auction>> GetAllAuctionsByStatusAsync(AuctionStatus status)
        {
            return await dbContext.Auctions
                .Where(x => x.Status == status)
                .Include(a => a.Product)
                    .ThenInclude(s => s.Seller)
                .Include(b => b.WinningBid)
                    .ThenInclude(a => a.Bidder)
                .ToListAsync();
        }

        public async Task<Auction?> GetAuctionByIdAsync(Guid id)
        {
            return await dbContext.Auctions
                .Include(a => a.Product)
                    .ThenInclude(s => s.Seller)
                //.Include(b => b.Winner)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Auction?> AddAuctionAsync(Auction auction)
        {
            dbContext.Auctions.Add(auction);
            await dbContext.SaveChangesAsync();
            var saved_auction = await GetAuctionByIdAsync(auction.Id);
            return saved_auction;
        }

        public async Task<Auction> UpdateAuctionAsync(Auction auction)
        {
            dbContext.Auctions.Update(auction);
            await dbContext.SaveChangesAsync();
            return auction;
        }

        public async Task<Auction> DeleteAuctionAsync(Guid id)
        {
            var auction = await dbContext.Auctions.FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null)
            {
                return null;
            }
            dbContext.Auctions.Remove(auction);
            await dbContext.SaveChangesAsync();
            return auction;
        }

        public async Task<List<Auction>> GetAuctionsByUserIdAsync(Guid userId)
        {
            return await dbContext.Auctions
                .Where(b => b.Bids.Any(b => b.BidderId == userId) || b.Product.SellerId == userId)
                .Include(a => a.Product)
                .Include(a => a.Bids)
                .ToListAsync();
        }

        public async Task<Auction> GetAuctionDetailsByAuctionIdAsync(Guid auctionId)
        {
            var auction = await dbContext.Auctions
                .Include(a => a.Product)
                    .ThenInclude(s => s.Seller)
                .Include(b => b.WinningBid)
                    .ThenInclude(a => a.Bidder)
                .Include(b => b.Bids)
                    .ThenInclude(a => a.Bidder)
                .Include(u => u.LikedByUsers)
                    .ThenInclude(u => u.user)
                .FirstOrDefaultAsync(x => x.Id == auctionId);
            return auction;
        }

        public async Task<int> ActiveAuctionCountAsync()
        {
            int count = await dbContext.Auctions
                .Where(a => a.Status == AuctionStatus.Active)
                .CountAsync();

            return count;
        }
        public async Task<int> ScheduledAuctionCountAsync()
        {
            int count = await dbContext.Auctions
                .Where(a => a.Status == AuctionStatus.Scheduled)
                .CountAsync();

            return count;
        }
        public async Task<int> EndedAuctionCountAsync()
        {
            int count = await dbContext.Auctions
                .Where(a => a.Status == AuctionStatus.Ended)
                .CountAsync();

            return count;
        }
        public async Task<int> CancelledAuctionCountAsync()
        {
            int count = await dbContext.Auctions
                .Where(a => a.Status == AuctionStatus.Cancelled)
                .CountAsync();

            return count;
        }
    }
}
