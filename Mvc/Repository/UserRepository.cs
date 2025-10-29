using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Models;

namespace Mvc.Repository
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
    }

    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext dbContext) : base(dbContext) { }        

        public async Task<User?> GetByEmailAsync(string email)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

            return user;
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await dbContext.Set<User>().OrderBy(x => x.Id).ToListAsync();
        }
    }
}
