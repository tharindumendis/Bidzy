using Bidzy.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Bidzy.API.DTOs.userDtos
{
    public class UserAddDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
