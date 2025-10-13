using Microsoft.EntityFrameworkCore;

namespace Mvc.Data
{
    public class SQLiteDbContext : AppDbContext
    {
        public SQLiteDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}
