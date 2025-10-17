using Bidzy.API.DTOs.user;
using Bidzy.Domain.Enum;

namespace Bidzy.API.DTOs.user
{
    public class UserUpdateDto
    {
        public string? FullName {  get; set; }
        public string? Phone {  get; set; }
        public string? Role { get; set; }
    }
}
