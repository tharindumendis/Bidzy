using System.ComponentModel.DataAnnotations;
using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByIdWithFavAsync(Guid id);
        Task<User?> GetUserByEmailAsync([EmailAddress]String email);
        Task<User?> AddUserAsync(User user);
        Task<User?> UpdateUserAsync(User user);
        Task<User?> DeleteUserAsync(Guid id);
    }
}
