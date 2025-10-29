using Microsoft.EntityFrameworkCore;
using Mvc.Data;

namespace Mvc.Repository
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T data);
        Task UpdateAsync(T data);
        Task DeleteAsync(T data);
    }

    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext dbContext;

        public BaseRepository(AppDbContext db)
        {
            dbContext = db;        
        }

        public async Task<T?> GetByIdAsync(int id)       
            => await dbContext.Set<T>().FindAsync(id);

        public virtual async Task<IEnumerable<T>> GetAllAsync()
            => await dbContext.Set<T>().ToListAsync();

        public async Task AddAsync(T data)
        {
            await dbContext.Set<T>().AddAsync(data);
            await dbContext.SaveChangesAsync();
        }
        public async Task UpdateAsync(T data)
        {
            dbContext.Set<T>().Update(data);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T data)
        {
            dbContext.Set<T>().Remove(data);
            await dbContext.SaveChangesAsync();
        }
    }
}
