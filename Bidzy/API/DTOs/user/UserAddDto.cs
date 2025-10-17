using Bidzy.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Bidzy.API.DTOs.user
{
    public class UserAddDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public IFormFile file { get; set; }
    }
}
