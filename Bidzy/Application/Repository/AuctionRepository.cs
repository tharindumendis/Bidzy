using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;
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
                .Include(b => b.Winner)
                .ToListAsync();
        }

        public async Task<Auction?> GetAuctionByIdAsync(Guid id)
        {
            return await dbContext.Auctions
                .Include(a => a.Product)
                    .ThenInclude(s => s.Seller)
                .Include(b => b.Winner)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Auction?> AddAuctionAsync(Auction auction)
        {
            dbContext.Auctions.Add(auction);
            await dbContext.SaveChangesAsync();
            return auction;
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

    }
}
