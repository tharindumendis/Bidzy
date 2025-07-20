using Bidzy.API.Dto;
using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IBidRepository
    {
        Task<Bid> GetBidByIdAsync(Guid bidId);
        Task<List<Bid>> GetAllBidsAsync();
        Task AddBidAsync(Bid bid);
        Task UpdateBidAsync(Bid bid);
        Task DeleteBidAsync(Guid bidId);
    }
}

