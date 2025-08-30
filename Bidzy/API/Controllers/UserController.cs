using Bidzy.API.DTOs;
using Bidzy.API.DTOs.userDtos;
using Bidzy.Application;
using Bidzy.Application.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserRepository userRepository, IAuthService authService, ISearchhistoryRepository searchhistoryRepository) : ControllerBase
    {
        private readonly IUserRepository userRepository = userRepository;
        private readonly IAuthService authService = authService;
        private readonly ISearchhistoryRepository searchhistoryRepository = searchhistoryRepository;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userRepository.GetAllUsersAsync();
            return Ok(users.Select(u => u.ToReadDto()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([FromRoute] Guid id)
        {
            var user = await userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user.ToReadDto());
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UserUpdateDto userUpdateDto)
        {
            var user = await userRepository.GetUserByIdAsync((Guid)id);
            if (user == null)
            {
                return NotFound();
            }
            user.UpdateEntity(userUpdateDto);
            var updatedUser = await userRepository.UpdateUserAsync(user);
            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            var user = await userRepository.DeleteUserAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetUserProfile([FromRoute] Guid id)
        {
            var user = await userRepository.GetUserByIdWithFavAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user.ToProfileDto());
        }

        [HttpDelete ("clear/{userId}")]
        public async Task<IActionResult> ClearSearchHistoryAsync(Guid userId)
        {
            await searchhistoryRepository.ClearSearchHistoryAsync(userId);
            return NoContent();
        }
    }
}
