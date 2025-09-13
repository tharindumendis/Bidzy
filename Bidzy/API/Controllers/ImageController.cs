using Bidzy.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Bidzy.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImageController(IImageService imageService) : ControllerBase
    {
        private readonly IImageService imageService = imageService;

        private readonly string _baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        [Authorize]
        [HttpPost("upload")]
        public IActionResult UploadImage(IFormFile file, [FromQuery] string type, [FromQuery] string entityId)
        {
            var imageUrl = imageService.UploadImage(file, type, entityId);
            return Ok(new { imageUrl });
        }

        [HttpGet("{type}/{entityId}")]
        public IActionResult GetImage(string type, string entityId)
        {
            var folderPath = Path.Combine(_baseFolder, type.ToLower());
            var filePath = Directory.GetFiles(folderPath)
                .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == entityId);

            if (filePath == null)
                return NotFound();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
                contentType = "application/octet-stream";

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, contentType);
        }

    }
}
