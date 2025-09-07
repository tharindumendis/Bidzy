using System.Security.Claims;
using Bidzy.API.DTOs;
using Bidzy.API.DTOs.userDtos;
using Bidzy.Application;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services;
using Bidzy.Application.Services.Auth;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Crypto.Generators;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IUserRepository userRepository, IAuthService authService, IEmailJobService emailJobService, IMemoryCache cache, IImageService imageService) : ControllerBase
    {
        private readonly IUserRepository userRepository = userRepository;
        private readonly IAuthService authService = authService;
        private readonly IEmailJobService emailJobService = emailJobService;
        private readonly IImageService imageService = imageService;
        private readonly Cache _cache = new(cache);



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
        public async Task<IActionResult> AddUser([FromForm] UserAddDto userAddDto)
        {
            bool isValidEmail = _cache.ValidateValidateEmail(userAddDto.Email);
            if (!isValidEmail) 
            { 
                return BadRequest();
            }

            userAddDto.Password = PasswordHasher.Hash(userAddDto.Password);
            var entity = userAddDto.ToEntity();
            var user = await userRepository.AddUserAsync(entity);
            if (user == null)
            {
                return BadRequest();
            }
            await imageService.UploadImage(userAddDto.file, "profile", user.Id.ToString());

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
        [HttpPost("exitemail")]
        public async Task<IActionResult> CheckExitEmail([FromBody] ExitEmailDto emailDto)
        {
            bool isExists = await userRepository.IsExistByUserEmailAsync(emailDto.Email);
            if (!isExists)
            {
                var otp = new Random().Next(100000, 999999).ToString();
                _cache.StoreOtp(emailDto.Email,otp);
                await emailJobService.SendOTP(otp, emailDto.Email);

            }
            return Ok(new { exists = isExists });
        }
        [HttpPost("otp")]
        public async Task<IActionResult> CheckOtp([FromBody] ExitEmailDto dto)
        {

            if (dto.OTP != null && _cache.ValidateOtp(dto.Email, dto.OTP))
            {
                _cache.StoreValidEmail(dto.Email);
                return Ok(new {success = true});
            }
            return BadRequest(new { success = false });
        }
  
    }
    public class Cache(IMemoryCache cache)
    {
        private readonly IMemoryCache _cache = cache;

        public void StoreOtp(string email, string otp)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); // OTP expires in 5 mins

            _cache.Set(email, otp, cacheEntryOptions);
        }

        public bool ValidateOtp(string email, string inputOtp)
        {
            if (_cache.TryGetValue(email, out string storedOtp))
            {
                return storedOtp == inputOtp;
            }
            return false;
        }
        public void StoreValidEmail(string email)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // OTP expires in 5 mins

            _cache.Set(email, "valid", cacheEntryOptions);
        }

        public bool ValidateValidateEmail(string email)
        {
            if (_cache.TryGetValue(email, out string value))
            {
                return "valid" == value;
            }
            return false;
        }
    }

}
