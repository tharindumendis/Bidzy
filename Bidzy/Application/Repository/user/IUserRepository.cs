using System.ComponentModel.DataAnnotations;
using Bidzy.Domain.Entities;

namespace Bidzy.Application.Repository.User
{
    public interface IUserRepository
    {
        Task<List<Domain.Entities.User>> GetAllUsersAsync();
        Task<Domain.Entities.User?> GetUserByIdAsync(Guid id);
        Task<Domain.Entities.User?> GetUserByIdWithFavAsync(Guid id);
        Task<Domain.Entities.User?> GetUserByEmailAsync([EmailAddress]string email);
        Task<Domain.Entities.User?> AddUserAsync(Domain.Entities.User user);
        Task<Domain.Entities.User?> UpdateUserAsync(Domain.Entities.User user);
        Task<Domain.Entities.User?> DeleteUserAsync(Guid id);
        Task<bool> IsExistByUserEmailAsync(string email);
    }
}
