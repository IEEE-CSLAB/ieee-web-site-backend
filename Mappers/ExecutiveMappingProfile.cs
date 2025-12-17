using AutoMapper;
using IEEEBackend.Dtos.Executive;
using IEEEBackend.Models;

namespace IEEEBackend.Mappers;

public class ExecutiveMappingProfile : Profile
{
    public ExecutiveMappingProfile()
    {
        CreateMap<Executive, ExecutiveDto>()
            .ForMember(dest => dest.CommitteeName, opt => opt.MapFrom(src => src.Committee.Name));
        CreateMap<CreateExecutiveDto, Executive>();
        CreateMap<UpdateExecutiveDto, Executive>();
    }
}

