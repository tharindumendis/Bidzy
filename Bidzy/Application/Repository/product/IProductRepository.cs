using Bidzy.Domain.Entities;

namespace Bidzy.Application.Repository.Product
{
    public interface IProductRepository
    {
        Task<List<Domain.Entities.Product>> GetAllProductsAsync();
        Task<Domain.Entities.Product?> GetProductsByIdAsync(Guid id);
        Task<Domain.Entities.Product?> AddProductsAsync(Domain.Entities.Product product);
        Task<Domain.Entities.Product?> UpdateProductsAsync(Domain.Entities.Product product);
        Task<Domain.Entities.Product?> DeleteProductsAsync(Guid id);
        Task<List<Domain.Entities.Product>> GetProductsByUserIdAsync(Guid sellerId);
    }
}
