using Microsoft.EntityFrameworkCore;
using Mvc.Models;

namespace Mvc.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Project> Projects { get; set; }
    }
}
