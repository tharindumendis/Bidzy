using Bidzy.Domain.Enum;

namespace Bidzy.API.DTOs.userDtos
{
    public class UserUpdateDto
    {
        public string? FullName {  get; set; }
        public string? Phone {  get; set; }
        public string? Role { get; set; }
    }
}
