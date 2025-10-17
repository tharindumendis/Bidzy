using Bidzy.API.DTOs.Admin;
using Bidzy.Domain.Entities;

namespace Bidzy.Application.Services.Admin
{
    public interface IAdminService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<bool> ActivateUserAsync(Guid userId);
        Task<bool> DeactivateUserAsync(Guid userId);

        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> ActivateProductAsync(Guid productId);
        Task<Product?> DeactivateProductAsync(Guid productId);

        Task<List<Auction>> GetAllAuctionsAsync();
        Task<Auction?> CancelAuctionAsync(Guid auctionId);

        Task<SiteAnalyticsDto> GetSiteAnalyticsAsync();

        Task<User> AddAdminUserAsync(AddAdminDto addAdminDto);
    }
}
