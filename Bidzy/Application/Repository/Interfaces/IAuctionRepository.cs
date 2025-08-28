using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IAuctionRepository
    {
        Task<Auction?> GetAuctionByIdAsync(Guid id);
        Task<List<Auction>> GetAllAuctionsAsync();
        Task<Auction?> AddAuctionAsync(Auction auction);
        Task<Auction> UpdateAuctionAsync(Auction auction);
        Task<Auction> DeleteAuctionAsync(Guid id);
        Task<List<Auction>> GetAllAuctionsByStatusAsync(AuctionStatus status);
        Task<List<Auction>> GetAuctionsByUserIdAsync(Guid userId);
    }
}
