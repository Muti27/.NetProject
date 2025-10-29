using Mvc.Data;
using Mvc.Models;

namespace Mvc.Repository
{
    public interface IProjectRepository
    {

    }

    public class ProjectRepository : BaseRepository<Project>, IProjectRepository
    {
        public ProjectRepository(AppDbContext db) : base(db) { }
    }
}
