using System.Security.Claims;
using Bidzy.API.DTOs;
using Bidzy.API.DTOs.userDtos;
using Bidzy.Application;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services.Auth;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Generators;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IUserRepository userRepository, IAuthService authService) : ControllerBase
    {
        private readonly IUserRepository userRepository = userRepository;
        private readonly IAuthService authService = authService;



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await userRepository.GetUserByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }
            

            // Replace this with proper password hashing check
            if (!PasswordHasher.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            string token = authService.GenerateJwtToken(user.Id, user.Email, user.Role.ToString());
            return Ok(new { token });
        }
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserAddDto userAddDto)
        {
            userAddDto.PasswordHash = PasswordHasher.Hash(userAddDto.PasswordHash);
            var entity = userAddDto.ToEntity();
            var user = await userRepository.AddUserAsync(entity);
            return Ok(user.ToReadDto());
        }
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;


            var user = await userRepository.GetUserByIdWithFavAsync(Guid.Parse(userId));
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user.ToProfileDto());

        }
    }

}
