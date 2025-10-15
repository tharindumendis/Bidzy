using Bidzy.Domain.Entities;
using Bidzy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Repository.User
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext dbContext;
        public UserRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<List<Domain.Entities.User>> GetAllUsersAsync()
        {
            return await dbContext.Users.ToListAsync();
        }

        public async Task<Domain.Entities.User?> GetUserByIdAsync(Guid id)
        {
            return await dbContext.Users.FindAsync(id);
        }
        public async Task<Domain.Entities.User?> GetUserByIdWithFavAsync(Guid id)
        {
            return await dbContext.Users.Where(x => x.Id == id)
                .Include(u => u.AuctionLikes)
                .ThenInclude(f => f.auction)
                .ThenInclude(a => a.Product)
                .FirstOrDefaultAsync();
        }
        public async Task<Domain.Entities.User?> GetUserByEmailAsync(string email)
        {
            return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<bool> IsExistByUserEmailAsync(string email)
        {
            return await dbContext.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<Domain.Entities.User?> AddUserAsync(Domain.Entities.User user)
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<Domain.Entities.User?> DeleteUserAsync(Guid id)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return null;
            }
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<Domain.Entities.User?> UpdateUserAsync(Domain.Entities.User user)
        {
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
            return user;
        }
    }
}
