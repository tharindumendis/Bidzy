using Microsoft.OpenApi.Any;

namespace Bidzy.Application.DTOs
{
    public class HubSubscribeData
    {
        public string[]? GroupIds { get; set; } = Array.Empty<string>();
        public string? UserId { get; set; }
        public object? Data { get; set; }

    }
}
