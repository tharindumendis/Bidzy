using Bidzy.API.DTOs.userDtos;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;

namespace Bidzy.API.DTOs
{
    public static class UserDtoMapper
    {
        public static UserReadDto ToReadDto(this User user )
        {
            return new UserReadDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }

        public static User ToEntity(this UserAddDto userAddDto)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                FullName = userAddDto.FullName,
                Email = userAddDto.Email,
                Phone = userAddDto.Phone,
                PasswordHash = userAddDto.PasswordHash,
                Role = Enum.TryParse<UserRole>(userAddDto.Role, out var role) ? role : UserRole.Bidder, // Default to Bidder if parsing fails
            };
        }

        public static void UpdateEntity(this User user, UserUpdateDto userUpdateDto)
        {
            if (!string.IsNullOrEmpty(userUpdateDto.FullName))
            {
                user.FullName = userUpdateDto.FullName;
            }
            if (!string.IsNullOrEmpty(userUpdateDto.Phone))
            {
                user.Phone = userUpdateDto.Phone;
            }
            if (!string.IsNullOrEmpty(userUpdateDto.Role) && Enum.TryParse<UserRole>(userUpdateDto.Role,true, out var role))
            {
                user.Role = role;
            }
        }

        public static userProfile ToProfileDto (this User user)
        {
            return new userProfile
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
