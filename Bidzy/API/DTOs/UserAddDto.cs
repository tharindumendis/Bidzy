using Bidzy.Domain.Enum;

namespace Bidzy.API.Dto
{
    public class UserAddDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
    }
}
