using Bidzy.API.DTOs;
using Bidzy.API.DTOs.bidDtos;
using Bidzy.Domain.Enties;

namespace Bidzy.Application.Services
{
    public interface IBidService
    {
        Task<List<Bid>> GetAllBidsByUser(Guid userId);
        Task<Bid?> PlaceBid(Bid dto);
        Task<PagedResult<Bid>> GetPagedBidsByUserAsync(Guid userId, int page, int pageSize);

    }
}
