using Bidzy.API.Dto;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<Auction?> GetAuctionByIdAsync(Guid id)
        {
            return await dbContext.Auctions
                .Include(a => a.Product)
                    .ThenInclude(p => p.Seller)
                .Include(a => a.Winner)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        public async Task<List<Auction>> GetAllAuctionsAsync()
        {
            return await dbContext.Auctions
                .Include(a => a.Product)
                    .ThenInclude(p => p.Seller)
                .Include(a => a.Winner)
                .ToListAsync();
        }
        public async Task<Auction?> AddAuctionAsync(AuctionAddDto dto)
        {
            var product = await dbContext.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);
            if (product == null)
            {
                return null;
            }

            var auctionEntity = new Auction
            {
                ProductId = dto.ProductId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                MinimumBid = dto.MinimumBid,
                Status = AuctionStatus.Scheduled,
                Product = product
            };
            // have to reduce db calls
            try
            {
                dbContext.Auctions.Add(auctionEntity);
                await dbContext.SaveChangesAsync();
                await dbContext.Entry(auctionEntity)
                    .Reference(a => a.Product)
                    .LoadAsync();
                await dbContext.Entry(auctionEntity.Product)
                    .Reference(p => p.Seller)
                    .LoadAsync();
                return auctionEntity;
            }
            catch
            {
                return null;
            }
        }
        public async Task<bool> UpdateAuctionAsync(Guid id, Auction dto)
        {
            var auction = await dbContext.Auctions.FindAsync(id);
            if (auction == null)
            {
                return false;
            }

            var product = await dbContext.Products.FindAsync(dto.ProductId);
            if (product == null)
            {
                return false;
            }

            auction.ProductId = dto.ProductId;
            auction.StartTime = dto.StartTime;
            auction.EndTime = dto.EndTime;
            auction.MinimumBid = dto.MinimumBid;
            auction.Status = dto.Status;
            auction.WinnerId = dto.WinnerId;

            try
            {
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> DeleteAuctionAsync(Guid id)
        {
            var auction = await dbContext.Auctions.FindAsync(id);
            if (auction == null)
            {
                return false;
            }

            try
            {
                dbContext.Auctions.Remove(auction);
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
