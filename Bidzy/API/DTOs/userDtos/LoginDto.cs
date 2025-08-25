using System.ComponentModel.DataAnnotations;

namespace Bidzy.API.DTOs.userDtos
{
    public class LoginDto
    {
        [EmailAddress]
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
