using System.Security.Claims;
using Bidzy.API.DTOs;
using Bidzy.API.DTOs.appReviewDtos;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppReviewController : ControllerBase
    {
        private readonly IAppReviewRepository appReviewRepository;
        private readonly IUserRepository userRepository;

        public AppReviewController(IAppReviewRepository appReviewRepository, IUserRepository userRepository)
        {
            this.appReviewRepository = appReviewRepository;
            this.userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reviews = await appReviewRepository.GetAllAsync();

            return Ok(reviews);
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var review = await appReviewRepository.GetByIdAsync(id);
            if (review == null) return NotFound();

            return Ok(new AppReviewReadDto
            {
                Id = review.Id.ToString(),
                FullName = review.FullName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            });
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(AppReviewCreateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            User user = await userRepository.GetUserByIdAsync(Guid.Parse(userId));
            var review = new AppReview
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(userId),
                FullName = user.FullName,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            await appReviewRepository.AddAsync(review);

            var resultDto = new AppReviewReadDto
            {
                Id = review.Id.ToString(),
                FullName = review.FullName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };

            return Ok(resultDto);
        }
    }
}
