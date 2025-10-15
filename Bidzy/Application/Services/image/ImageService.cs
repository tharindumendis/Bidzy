using Microsoft.AspNetCore.StaticFiles;

namespace Bidzy.Application.Services.Image
{
    public class ImageService : IImageService
    {
        private readonly string _baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public async Task<string> UploadImage(IFormFile file, string type, string entityId)
        {
            if (file == null || file.Length == 0)
                return "No file uploaded.";

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
                return "Unsupported file type.";

            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(entityId))
                return "Missing type or entityId.";

            var folderPath = Path.Combine(_baseFolder, type.ToLower());
            Directory.CreateDirectory(folderPath);

            var fileName = $"{entityId}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/Image/{type}/{entityId}";
        }

        public (byte[] FileBytes, string ContentType)? GetImage(string type, string entityId)
        {
            var folderPath = Path.Combine(_baseFolder, type.ToLower());
            var filePath = Directory.GetFiles(folderPath)
                .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == entityId);

            if (filePath == null)
                return null;

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
                contentType = "application/octet-stream";

            var bytes = File.ReadAllBytes(filePath);
            return (bytes, contentType);
        }
    }
}