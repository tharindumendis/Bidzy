using System.Security.Claims;

namespace Bidzy.Application.Services.Auth
{
    public interface IAuthService
    {
        string GenerateJwtToken(Guid userId, string email, string role);
        ClaimsPrincipal ValidateToken(string token);
    }
}
