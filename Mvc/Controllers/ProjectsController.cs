using Microsoft.AspNetCore.Mvc;
using Mvc.Data;
using Mvc.Models;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProjectsController(AppDbContext appDbContext)
    {
        _context = appDbContext;
    }

    [HttpGet]
    public IActionResult GetAllProjects()
    {      
        return Ok(_context.Projects.ToList());
    }

    [HttpGet("{id}")]
    public IActionResult GetProjectById(int id)
    {
        var project = _context.Projects.Find(id);
        if (project == null)
        {
            return NotFound("project not found.");
        }

        return Ok(project);
    }

    [HttpPost]
    public IActionResult Create(Project project)
    {
        _context.Projects.Add(project);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Project updatedProject)
    {
        var project = _context.Projects.Find(id);
        if (project == null)
        {
            return NotFound();
        }

        project.Name = updatedProject.Name;
        project.Description = updatedProject.Description;
        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var project = _context.Projects.Find(id);
        if (project == null)
        {
            return NotFound();
        }

        _context.Projects.Remove(project);
        _context.SaveChanges();

        return NoContent();
    }
}
