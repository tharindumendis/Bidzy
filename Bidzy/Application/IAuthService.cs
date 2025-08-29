using System.Security.Claims;

namespace Bidzy.Application
{
    public interface IAuthService
    {
        string GenerateJwtToken(Guid userId, string email, string role);
        ClaimsPrincipal ValidateToken(string token);
    }
}
