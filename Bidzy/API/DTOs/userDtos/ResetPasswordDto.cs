using System.ComponentModel.DataAnnotations;

namespace Bidzy.API.DTOs.userDtos
{
    public class ResetPasswordDto
    {
        [EmailAddress]
        public required string Email { get; set; }
        public string? OTP { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}
