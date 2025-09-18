using Bidzy.API.DTOs;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IAuctionRepository
    {
        Task<Auction?> GetAuctionByIdAsync(Guid id);
        Task<Auction?> GetAuctionByIdLowAsync(Guid id);
        Task<List<Auction>> GetAllAuctionsAsync();
        Task<Auction?> AddAuctionAsync(Auction auction);
        Task<Auction> UpdateAuctionAsync(Auction auction);
        Task<Auction> DeleteAuctionAsync(Guid id);
        Task<List<Auction>> GetAllAuctionsByStatusAsync(AuctionStatus status);
        Task<List<Auction>> GetAllActiveOrScheduledAuctionAsync();
        Task<List<Auction>> GetAuctionsByUserIdAsync(Guid userId);
        Task<Auction> GetAuctionDetailsByAuctionIdAsync(Guid auctionId);
        Task<int> ActiveAuctionCountAsync();
        Task<int> ScheduledAuctionCountAsync();
        Task<int> CancelledAuctionCountAsync();
        Task<int> EndedAuctionCountAsync();
        Task<List<Auction>> GetAllShopAuctionDetailsAsync(Guid id);
        Task<PagedResult<Auction>> SearchAuctionsAsync(AuctionSearchParams searchParams);
    }
}
