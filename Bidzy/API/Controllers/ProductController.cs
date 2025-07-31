using Bidzy.API.DTOs;
using Bidzy.API.DTOs.productsDtos;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository productRepository;
        private readonly ITagRepository tagRepository;

        public ProductController(IProductRepository productRepository, ITagRepository tagRepository )
        {
            this.productRepository = productRepository;
            this.tagRepository = tagRepository;
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
            var tags = productAddDto.Tags != null
                ? await tagRepository.ResolveTagsAsync(productAddDto.Tags)
                : new List<Tag>();
            var product = productAddDto.ToEntity(tags);
            var createdProduct = await productRepository.AddProductsAsync(product);

            return Ok(createdProduct);
            
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromBody] ProductsUpdateDto productUpdateDto)
        {
            var product = await productRepository.GetProductsByIdAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }
            if (productUpdateDto.Tags != null)
            {
                var tags = await tagRepository.ResolveTagsAsync(productUpdateDto.Tags);
                product.Tags.Clear();
                foreach (var tag in tags)
                {
                    product.Tags.Add(tag);
                }
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
