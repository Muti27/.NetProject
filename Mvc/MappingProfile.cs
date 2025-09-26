using AutoMapper;
using Mvc.Dtos;
using Mvc.Models;

namespace Mvc
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            //entity -> dto
            CreateMap<Project, ProjectDto>();

            //dto -> entity
            CreateMap<CreateProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreateTime, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreateTime, opt => opt.Ignore());
        }
    }
}
