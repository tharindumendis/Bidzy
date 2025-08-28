using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductsByIdAsync(Guid id);
        Task<Product?> AddProductsAsync(Product product);
        Task<Product?> UpdateProductsAsync(Product product);
        Task<Product?> DeleteProductsAsync(Guid id);
        Task<List<Product>> GetProductsByUserIdAsync(Guid sellerId);
    }
}
