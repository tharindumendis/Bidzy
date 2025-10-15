using Bidzy.API.DTOs.Common;
using Bidzy.Domain.Entities;

namespace Bidzy.Application.Repository.Bid
{
    public interface IBidRepository
    {
        Task<Domain.Entities.Bid?> GetBidByIdAsync(Guid bidId);
        Task<List<Domain.Entities.Bid>> GetBiddersByAuctionIdAsync(Guid auctionId);
        Task<List<Domain.Entities.Bid>> GetBidsByUserIdAsync(Guid userId);
        Task<List<Domain.Entities.Bid>> GetAllBidsAsync();
        Task<Domain.Entities.Bid?> AddBidAsync( Domain.Entities.Bid bid);
        Task<Domain.Entities.Bid?> UpdateBidAsync(Domain.Entities.Bid bid);
        Task<Domain.Entities.Bid?> DeleteBidAsync(Guid bidId);
        IQueryable<Domain.Entities.Bid> Query();
        Task<Domain.Entities.Bid?> GetWinningBidAsync(Guid auctionId, DateTime endTime);
        Task<PagedResult<Domain.Entities.Bid>> GetPagedBidsByUserAsync(Guid userId, int page, int pageSize);
    }
}

