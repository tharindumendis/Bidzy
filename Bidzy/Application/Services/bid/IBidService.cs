using Bidzy.API.DTOs.bid;
using Bidzy.API.DTOs.Common;
using Bidzy.Domain.Entities;

namespace Bidzy.Application.Services.Bid
{
    public interface IBidService
    {
        Task<List<Domain.Entities.Bid>> GetAllBidsByUser(Guid userId);
        Task<Domain.Entities.Bid?> PlaceBid(Domain.Entities.Bid dto);
        Task<PagedResult<Domain.Entities.Bid>> GetPagedBidsByUserAsync(Guid userId, int page, int pageSize);

        Task<BidderActivityDto> GetBidderActivityAsync(Guid userId);

    }
}
