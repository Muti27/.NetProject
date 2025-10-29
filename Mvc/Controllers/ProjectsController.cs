using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Mvc.Models;
using Mvc.Models.Dtos;
using Mvc.Services;

namespace Mvc.Controllers
{
    [ApiController]
    public class ProjectsController : Controller
    {
        private readonly IProjectService projectService;

        public ProjectsController(IProjectService service)
        {
            projectService = service;
        }

        public IActionResult Projects(int? projectId)
        {
            if (projectId == null)
            {

            }

            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = await projectService.Create(createDto);
            if (!result.Success)
            {
                return View();
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Update(UpdateProjectDto updateDto)
        {
            if (!ModelState.IsValid)
                return View();

            var result = await projectService.Update(updateDto);
            if (!result.Success)
            {
                return View();
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid)
                return View();

            var result = await projectService.Delete(id);
            if (!result.Success)
            {
                return View();
            }

            return View();
        }
    }
}
