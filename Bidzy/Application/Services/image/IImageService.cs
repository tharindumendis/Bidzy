namespace Bidzy.Application.Services.Image
{
    public interface IImageService
    {
        Task<string> UploadImage(IFormFile file, string type, string entityId);
        (byte[] FileBytes, string ContentType)? GetImage(string type, string entityId);
    }
}
