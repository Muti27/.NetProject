using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Dtos;
using Mvc.Models;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ProjectsController(AppDbContext appDbContext, IMapper mapper)
    {
        _context = appDbContext;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dtos = await _context.Projects
            .AsQueryable()
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(dtos);
    }

    [HttpGet("ById/{id}")]
    public IActionResult GetProjectById(int id)
    {
        var dtos = _context.Projects
            .Where(p => p.Id == id)
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .FirstOrDefault();

        if (dtos == null)
            return NotFound();

        return Ok(dtos);
    }

    [HttpGet("ByName/{name}")]
    public IActionResult GetProjectByName(string name)
    {
        var dtos = _context.Projects
            .Where(p => p.Name == name)
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .FirstOrDefault();

        if (dtos == null)
            return NotFound();

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateProjectDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var project = _mapper.Map<Project>(createDto);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<ProjectDto>(project);
        return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, resultDto);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, UpdateProjectDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var project = _context.Projects.Find(id);
        if (project == null)
            return NotFound();

        project = _mapper.Map<Project>(updateDto);
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
