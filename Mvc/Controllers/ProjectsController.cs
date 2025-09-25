using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllProjects()
    {
        var projects = new[]
        {
            new { Id = 1, Name = "First Project", Description = "Demo" },
            new { Id = 2, Name = "Second Project", Description = "Demo 2" }
        };
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public IActionResult GetProjectById(int id)
    {
        return Ok(new { Id = id, Name = $"Project {id}", Description = "Detail" });
    }
}
