using Microsoft.EntityFrameworkCore;

namespace Mvc.Data
{
    public class PostgresDbContext : AppDbContext
    {
        public PostgresDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
    }
}
