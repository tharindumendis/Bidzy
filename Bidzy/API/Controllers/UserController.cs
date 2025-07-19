using Bidzy.API.Dto;
using Bidzy.Data;
using Bidzy.Domain.Enum;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public UserController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult GetAllUser()
        {
            var users = dbContext.Users.ToList();
            if (User == null)
            {
                return NotFound();
            }
            return Ok(users);
        }
        [HttpPost]
        public IActionResult AddUser(UserAddDto addUserDto)
        {
            // Validate role
            if (!Enum.IsDefined(typeof(UserRole), addUserDto.Role))
                return BadRequest("Invalid user role.");

            // Normalize email
            var normalizedEmail = addUserDto.Email.Trim().ToLower();

            // Check for existing user
            if (dbContext.Users.Any(u => u.Email.ToLower() == normalizedEmail))
                return BadRequest("This email is already registered.");

            // Create user entity
            var userEntity = new User
            {
                FullName = addUserDto.FullName.Trim(),
                Email = normalizedEmail,
                Phone = addUserDto.Phone,
                PasswordHash = addUserDto.PasswordHash, // Consider hashing here
                Role = addUserDto.Role,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                dbContext.Users.Add(userEntity);
                dbContext.SaveChanges();
                return Ok(userEntity);
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




    }
}
