using System.Security.Claims;

namespace Bidzy.Application
{
    public interface IAuthService
    {
        string GenerateJwtToken(string userId);
        ClaimsPrincipal ValidateToken(string token);
    }
}
