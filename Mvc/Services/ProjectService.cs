using AutoMapper;
using Mvc.Models;
using Mvc.Models.Dtos;
using Mvc.Repository;

namespace Mvc.Services
{
    public interface IProjectService
    {
        public Task<ServiceResult<ProjectDto>> Create(CreateProjectDto dto);
        public Task<ServiceResult<ProjectDto>> Update(UpdateProjectDto dto);
        public Task<ServiceResult<ProjectDto>> Delete(int id);
    }

    public class ProjectService : IProjectService
    {
        private readonly ProjectRepository repository;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public ProjectService(ProjectRepository repository, IMapper mapper, ILogger logger)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<ServiceResult<ProjectDto>> Create(CreateProjectDto dto)
        {
            var project = mapper.Map<Project>(dto);

            await repository.AddAsync(project);

            var resultDto = mapper.Map<ProjectDto>(project);

            return ServiceResult<ProjectDto>.Ok(resultDto);
        }               

        public async Task<ServiceResult<ProjectDto>> Update(UpdateProjectDto dto)
        {
            var project = await repository.GetByIdAsync(dto.Id);
            if (project == null)
                return ServiceResult<ProjectDto>.Failed("");

            project = mapper.Map<Project>(dto);
            
            await repository.UpdateAsync(project);

            return ServiceResult<ProjectDto>.Ok(null);
        }

        public async Task<ServiceResult<ProjectDto>> Delete(int id)
        {
            var project = await repository.GetByIdAsync(id);
            if (project == null)
                return ServiceResult<ProjectDto>.Failed("");

            await repository.DeleteAsync(project);

            return ServiceResult<ProjectDto>.Ok(null);
        }
    }
}
