using Bidzy.API.DTOs.auctionDtos;
using Bidzy.Domain.Enties;

namespace Bidzy.Application.Services.AuctionEngine
{
    public interface IAuctionEngine
    {
        Task<Auction> CreateAuctionAsync(AuctionAddDto dto);
        Task StartAuctionAsync(Guid auctionId);
        Task EndAuctionAsync(Guid auctionId);
        Task CancelAuctionAsync(Guid auctionId);
    }
}
