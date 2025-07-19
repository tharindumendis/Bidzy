using Bidzy.API.Dto;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public ProductController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult getAllProduct()
        {
            var products = dbContext.products
                .Include(p => p.Seller) // Eagerly load the Seller navigation property
                .ToList();

            if (products == null)
            {
                return NotFound();
            }
            return Ok(products);
        }
        [HttpPost]
        public IActionResult AddProduct(ProductAddDto dto)
        {
            User user = dbContext.Users.Find(dto.SellerId);
            if (user == null) 
            {
                return BadRequest("Seller Id is invalid");
            }
            var productEntity = new Product
            {
                Title = dto.Title,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                SellerId = dto.SellerId
            };
            try
            {
                dbContext.products.Add(productEntity);
                dbContext.SaveChanges();
                return Ok(productEntity);
            }
            catch (DbUpdateException dbEx)
            {
                // Handle DB-specific errors (e.g., unique constraint violation)
                return StatusCode(500, "Database error occurred.");
            }
            catch (Exception ex)
            {
                // Log exception if needed
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
        [HttpPut("{id:guid}")]
        public IActionResult UpdateProduct(Guid id, ProductAddDto dto)
        {
            var product = dbContext.products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            var user = dbContext.Users.Find(dto.SellerId);
            if (user == null)
            {
                return BadRequest("Seller Id is invalid");
            }

            product.Title = dto.Title;
            product.Description = dto.Description;
            product.ImageUrl = dto.ImageUrl;
            product.SellerId = dto.SellerId;

            try
            {
                dbContext.SaveChanges();
                return Ok(product);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
        [HttpGet("{id:guid}")]
        public IActionResult GetProductById(Guid id)
        {
            var product = dbContext.products
                .Include(p => p.Seller)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteProduct(Guid id)
        {
            var product = dbContext.products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            dbContext.products.Remove(product);
            try
            {
                dbContext.SaveChanges();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
