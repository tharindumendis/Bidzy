using Bidzy.API.DTOs.auctionDtos;
using Bidzy.API.DTOs.Common;
using Bidzy.Application.DTOs;
using Bidzy.Domain.Entities;
using Bidzy.Domain.Enum;

namespace Bidzy.Application.Repository.Auction
{
    public interface IAuctionRepository
    {
        Task<Domain.Entities.Auction?> GetAuctionByIdAsync(Guid id);
        Task<List<Domain.Entities.Auction>> GetSuggestedAuctionsAsync(Guid userId);
        Task<List<AuctionWithMaxBidDto>> GetSuggestedAuctionsWithMaxBidAsync(Guid userId);
        Task<Domain.Entities.Auction?> GetAuctionByIdLowAsync(Guid id);
        Task<List<Domain.Entities.Auction>> GetAllAuctionsAsync();
        Task<List<AuctionWithMaxBidDto>> GetAllAuctionsWithMaxBidAsync();
        Task<Domain.Entities.Auction?> AddAuctionAsync(Domain.Entities.Auction auction);
        Task<Domain.Entities.Auction> UpdateAuctionAsync(Domain.Entities.Auction auction);
        Task<Domain.Entities.Auction> DeleteAuctionAsync(Guid id);
        Task<List<Domain.Entities.Auction>> GetAllAuctionsByStatusAsync(AuctionStatus status);
        Task<List<Domain.Entities.Auction>> GetAllActiveOrScheduledAuctionAsync();
        Task<List<AuctionWithMaxBidDto>> GetAllActiveOrScheduledAuctionsWithMaxBidAsync();
        Task<List<Domain.Entities.Auction>> GetAuctionsByUserIdAsync(Guid userId);
        Task<Domain.Entities.Auction> GetAuctionDetailsByAuctionIdAsync(Guid auctionId);
        Task<int> ActiveAuctionCountAsync();
        Task<int> ScheduledAuctionCountAsync();
        Task<int> CancelledAuctionCountAsync();
        Task<int> EndedAuctionCountAsync();
        Task<List<Domain.Entities.Auction>> GetAllShopAuctionDetailsAsync(Guid id);
        Task<PagedResult<Domain.Entities.Auction>> SearchAuctionsAsync(AuctionSearchParams searchParams);

        Task<IEnumerable<Domain.Entities.Auction>> GetFullAuctionsByIdsAsync(IEnumerable<Guid> auctionIds);
        Task<IEnumerable<Domain.Entities.Auction>> GetFullWonAuctionsByUserIdAsync(Guid userId);
    }
}
