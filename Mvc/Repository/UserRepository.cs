using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Models;

namespace Mvc.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task<List<User>> GetUserList();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var user = await dbContext.Users.FindAsync(id);

            return user;
        }

        public async Task<List<User>> GetUserList()
        {
            var list = await dbContext.Users.OrderBy(x => x.Id).ToListAsync();

            return list;
        }

        public async Task AddAsync(User user)
        {
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
        }
    }
}
