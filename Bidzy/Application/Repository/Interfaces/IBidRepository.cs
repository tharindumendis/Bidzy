using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IBidRepository
    {
        Task<Bid?> GetBidByIdAsync(Guid bidId);
        Task<List<Bid>> GetBiddersByAuctionIdAsync(Guid auctionId);
        Task<List<Bid>> GetAllBidsAsync();
        Task<Bid?> AddBidAsync(Bid bid);
        Task<Bid?> UpdateBidAsync(Bid bid);
        Task<Bid?> DeleteBidAsync(Guid bidId);
    }
}

