using Bidzy.API.DTOs.Admin;
using Bidzy.API.DTOs.userDtos;
using Bidzy.Domain.Entities;
using Bidzy.Domain.Enum;

namespace Bidzy.Application.Mappers
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
                ImageUrl = user.imageUrl,
                CreatedAt = user.CreatedAt
            };
        }

        public static User ToEntity(this UserAddDto userAddDto)
        {
            Guid newId = Guid.NewGuid();
            return new User
            {
                Id = newId,
                FullName = userAddDto.FullName,
                Email = userAddDto.Email,
                IsActive = true,
                Phone = userAddDto.PhoneNumber,
                PasswordHash = userAddDto.Password,
                imageUrl = "/Image/profile/"+newId.ToString(),
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
                Role = user.Role.ToString(),
                ImageUrl = user.imageUrl,
                FavoriteAuctions = user.AuctionLikes.Select(al => al.auctionId.ToString()).ToList(),
                CreatedAt = user.CreatedAt
            };
        }

        public static User ToEntity(this AddAdminDto addAdminDto)
        {
            Guid newId = Guid.NewGuid();
            return new User
            {
                Id = newId,
                FullName = addAdminDto.FullName,
                Email = addAdminDto.Email,
                Phone = addAdminDto.PhoneNumber,
                PasswordHash = addAdminDto.Password,
            };
        }
    }
}
