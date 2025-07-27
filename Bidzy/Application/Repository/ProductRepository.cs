using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ProductRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await dbContext.Products
                .Include(s => s.Seller)
                .ToListAsync();
        }

        public async Task<Product?> GetProductsByIdAsync(Guid id)
        {
            return await dbContext.Products
                .Include(s => s.Seller)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Product?> AddProductsAsync(Product product)
        {
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateProductsAsync(Product product)
        {
            dbContext.Products.Update(product);
            await dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> DeleteProductsAsync(Guid id)
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
    }
}
