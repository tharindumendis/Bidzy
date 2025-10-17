using System.ComponentModel.DataAnnotations;

namespace Bidzy.API.DTOs.user
{
    public class ExitEmailDto
    {
        [EmailAddress]
        public required string Email { get; set; }
        public string? OTP { get; set; }
        public string? Action { get; set; }
    }
}
