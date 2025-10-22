using System.Security.Claims;
using Bidzy.API.DTOs;
using Bidzy.API.DTOs.products;
using Bidzy.Application.Mappers;
using Bidzy.Application.Repository.Product;
using Bidzy.Application.Repository.Tag;
using Bidzy.Application.Services.Image;
using Bidzy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Bidzy.API.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IImageService imageService, IProductRepository productRepository, ITagRepository tagRepository) : ControllerBase
    {
        private readonly IProductRepository productRepository = productRepository;
        private readonly ITagRepository tagRepository = tagRepository;
        private readonly IImageService imageService = imageService;


        

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
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductAddDto productAddDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tags = productAddDto.Tags != null
                ? await tagRepository.ResolveTagsAsync(productAddDto.Tags)
                : new List<Tag>();
            var product = productAddDto.ToEntity(tags, Guid.Parse(userId));
            await imageService.UploadImage(productAddDto.file, "product", product.Id.ToString());
            var createdProduct = await productRepository.AddProductsAsync(product);

            return Ok(createdProduct);
            
        }
        [Authorize]
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

        [Authorize]
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

        [Authorize]
        [HttpGet("user/{sellerId}")]
        public async Task<IActionResult> GetProductsByUserId([FromRoute] Guid sellerId)
        {
            var products = await productRepository.GetProductsByUserIdAsync(sellerId);
            if (products == null || !products.Any())
            {
                return NotFound("No products found for the specified user.");
            }
            return Ok(products.Select(p => p.ToReadDto()));
        }
    }
}
