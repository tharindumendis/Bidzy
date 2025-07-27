using Bidzy.Domain.Enum;

namespace Bidzy.API.DTOs.userDtos
{
    public class UserReadDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
