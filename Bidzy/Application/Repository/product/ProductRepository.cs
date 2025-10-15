using Bidzy.Domain.Entities;
using Bidzy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Repository.Product
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ProductRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Domain.Entities.Product>> GetAllProductsAsync()
        {
            return await dbContext.Products
                .Include(s => s.Seller)
                .Include(t=> t.Tags)
                .ToListAsync();
        }

        public async Task<Domain.Entities.Product?> GetProductsByIdAsync(Guid id)
        {
            return await dbContext.Products
                .Include(s => s.Seller)
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Domain.Entities.Product?> AddProductsAsync(Domain.Entities.Product product)
        {
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<Domain.Entities.Product?> UpdateProductsAsync(Domain.Entities.Product product)
        {
            dbContext.Products.Update(product);
            await dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<Domain.Entities.Product?> DeleteProductsAsync(Guid id)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return null;
            }
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<List<Domain.Entities.Product>> GetProductsByUserIdAsync(Guid sellerId)
        {
            return await dbContext.Products
                .Where(x => x.SellerId == sellerId)
                .Include(s => s.Seller)
                .Include(t => t.Tags)
                .ToListAsync();
        }
    }
}
