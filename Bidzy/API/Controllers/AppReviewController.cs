using Bidzy.API.DTOs;
using Bidzy.Application.Services;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppReviewController : ControllerBase
    {
        private readonly AppReviewService _service;

        public AppReviewController(AppReviewService service)
        {
            _service = service;
        }

        // [HttpGet]
        // public async Task<IActionResult> GetAll()
        // {
        //     var reviews = await _service.GetAllReviews()
        //         .Select(r => new AppReviewReadDto
        //         {
        //             Id = r.Id,
        //             FullName = r.FullName,
        //             Rating = r.Rating,
        //             Comment = r.Comment,
        //             CreatedAt = r.CreatedAt
        //         });
        //     return Ok(reviews);
        // }

        // [HttpGet("{id}")]
        // public async  Task<IActionResult> Get(Guid id)
        // {
        //     var review = await _service.GetReview(id);
        //     if (review == null) return NotFound();

        //     return Ok(new AppReviewReadDto
        //     {
        //         Id = review.Id.ToString(),
        //         FullName = review.FullName,
        //         Rating = review.Rating,
        //         Comment = review.Comment,
        //         CreatedAt = review.CreatedAt
        //     });
        // }

//         [HttpPost]
//         public async Task<IActionResult> Create(AppReviewCreateDto dto)
//         {
//             var review = new AppReview
//             {
//                 Id = Guid.NewGuid(),
//                 UserId = dto.UserId,
//                 FullName = dto.FullName,
//                 Rating = dto.Rating,
//                 Comment = dto.Comment
//             };

//             await _service.AddReview(review);

//             var resultDto = new AppReviewReadDto
//             {
//                 Id = review.Id.ToString(),
//                 FullName = review.FullName,
//                 Rating = review.Rating,
//                 Comment = review.Comment,
//                 CreatedAt = review.CreatedAt
//             };

//           return ok(resultDto);
    }
}
