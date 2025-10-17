using Bidzy.API.Controllers;
using Bidzy.API.DTOs.Admin;
using Bidzy.Application.Mappers;
using Bidzy.Application.Repository.Auction;
using Bidzy.Application.Repository.Product;
using Bidzy.Application.Repository.User;
using Bidzy.Application.Services.Auth;
using Bidzy.Domain.Entities;
using Bidzy.Domain.Enum;
using MailKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Bidzy.Application.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository userRepository;
        private readonly IProductRepository productRepository;
        private readonly IAuctionRepository auctionRepository;

        public AdminService(IUserRepository userRepository, IProductRepository productRepository, IAuctionRepository auctionRepository)
        {
            this.userRepository = userRepository;
            this.productRepository = productRepository;
            this.auctionRepository = auctionRepository;
        }

        public async Task<bool> ActivateUserAsync(Guid userId)
        {
            var user = await userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return false;
            user.IsActive = true;
            var updatedUser = await userRepository.UpdateUserAsync(user);
            return updatedUser != null;
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            var user = await userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return false;
            user.IsActive = false;
            var updatedUser = await userRepository.UpdateUserAsync(user);
            return updatedUser != null;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await userRepository.GetAllUsersAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await userRepository.GetUserByIdAsync(userId);
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await productRepository.GetAllProductsAsync();
        }

        public async Task<Product?> ActivateProductAsync(Guid productId)
        {
            var product = await productRepository.GetProductsByIdAsync(productId);
            if (product == null)
                return null;
            product.IsActive = true;
            var updatedProduct = await productRepository.UpdateProductsAsync(product);
            return updatedProduct;
        }

        public async Task<Product?> DeactivateProductAsync(Guid productId)
        {
            var product = await productRepository.GetProductsByIdAsync(productId);
            if (product == null)
                return null;
            product.IsActive = false;
            var updatedProduct = await productRepository.UpdateProductsAsync(product);
            return updatedProduct;
        }

        public async Task<List<Auction>> GetAllAuctionsAsync()
        {
            return await auctionRepository.GetAllAuctionsAsync();
        }

        public async Task<Auction?> CancelAuctionAsync(Guid auctionId)
        {
            var auction = await auctionRepository.GetAuctionByIdAsync(auctionId);
            if (auction == null)
                return null;
            auction.Status = AuctionStatus.Cancelled;
            var updatedAuction = await auctionRepository.UpdateAuctionAsync(auction);
            return updatedAuction;
        }

        

        public async Task<SiteAnalyticsDto> GetSiteAnalyticsAsync()
        {
            var totalUsers = await userRepository.GetAllUsersAsync();
            var totalProducts = await productRepository.GetAllProductsAsync();
            var totalAuctions = await auctionRepository.GetAllAuctionsAsync();

            return new SiteAnalyticsDto
            {
                UserStats = new UserStatsDto
                {
                    Total = totalUsers.Count,
                    Active = totalUsers.Count(u => u.IsActive),
                    Inactive = totalUsers.Count(a => !a.IsActive),
                    NewThisMonth = totalUsers.Count(u => u.CreatedAt.Month == DateTime.UtcNow.Month)
                },

                ProductStats = new ProductStatsDto
                {
                    Total = totalProducts.Count,
                    Active = totalProducts.Count(a => a.IsActive),
                    Inactive = totalProducts.Count(b => !b.IsActive),
                },

                AuctionStats = new AuctionStatsDto
                {
                    Total = totalAuctions.Count,
                    Active = totalAuctions.Count(p => p.Status == AuctionStatus.Active),
                    Ended = totalAuctions.Count(p => p.Status == AuctionStatus.Ended),
                    Canceled = totalAuctions.Count(p => p.Status == AuctionStatus.Cancelled),
                    Revenue = totalAuctions.Where(a => a.Status == AuctionStatus.Ended
                                && a.WinningBid != null)
                                .Sum(a => a.WinningBid.Amount)
                }
            };

        }
        public async Task<User> AddAdminUserAsync(AddAdminDto addAdminDto)
        {
            addAdminDto.Password = PasswordHasher.Hash(addAdminDto.Password);
            var entity = addAdminDto.ToEntity();
            entity.Role = UserRole.Admin;
            entity.IsActive = false;
            entity.imageUrl = "/user/admin";
            var user = await userRepository.AddUserAsync(entity);

            if (user == null)
            {
                throw new InvalidOperationException("Failed to add admin user.");
            }

            return user;
        }
    }
}
