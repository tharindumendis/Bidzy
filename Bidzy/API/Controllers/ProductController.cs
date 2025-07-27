using Bidzy.API.DTOs;
using Bidzy.API.DTOs.productsDtos;
using Bidzy.Application.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository productRepository;

        public ProductController(IProductRepository productRepository)
        {
            this.productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await productRepository.GetAllProductsAsync();
            return Ok(products.Select(p => p.ToReadDto()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById([FromRoute] Guid id)
        {
            var product = await productRepository.GetProductsByIdAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }
            return Ok(product.ToReadDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductAddDto productAddDto)
        {
            var entity = productAddDto.ToEntity();
            var product = await productRepository.AddProductsAsync(entity);
            return Ok(product.ToReadDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromBody] ProductsUpdateDto productUpdateDto)
        {
            var product = await productRepository.GetProductsByIdAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }
            product.UpdateEntity(productUpdateDto);
            var updatedProduct = await productRepository.UpdateProductsAsync(product);
            return Ok(updatedProduct);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] Guid id)
        {
            var product = await productRepository.DeleteProductsAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }
            return NoContent();
        }
    }
}
