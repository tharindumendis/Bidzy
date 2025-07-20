using Bidzy.API.Dto;
using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IAuctionRepository
    {
        Task<Auction?> GetAuctionByIdAsync(Guid id);
        Task<List<Auction>> GetAllAuctionsAsync();
        Task<Auction?> AddAuctionAsync(AuctionAddDto dto);
        Task<bool> UpdateAuctionAsync(Guid id, Auction dto);
        Task<bool> DeleteAuctionAsync(Guid id);
    }
}
