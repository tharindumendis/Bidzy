using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Bidzy.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        public string GenerateJwtToken(Guid userId, string email, string role)
        {
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), // Subject: user ID
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // .NET-friendly user ID
        new Claim(ClaimTypes.Email, email), // Email claim
        new Claim(ClaimTypes.Role, role) // Role claim for [Authorize(Roles = "Admin")]
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("bidzyUltraSecureKey_2025!@#LongEnoughToPass"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "yourIssuer",
                audience: "yourAudience",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("bidzyUltraSecureKey_2025!@#LongEnoughToPass");

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "yourIssuer",
                ValidAudience = "yourAudience",
                ValidateLifetime = true
            }, out SecurityToken validatedToken);

            return principal;
        }
    }
}
