using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Bidzy.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ImageController : ControllerBase
    {
        private readonly string _baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string type, [FromQuery] string entityId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
                return BadRequest("Unsupported file type.");

            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(entityId))
                return BadRequest("Missing type or entityId.");

            var folderPath = Path.Combine(_baseFolder, type.ToLower());
            Directory.CreateDirectory(folderPath);

            var fileName = $"{entityId}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var imageUrl = $"/image/{type}/{entityId}";
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
