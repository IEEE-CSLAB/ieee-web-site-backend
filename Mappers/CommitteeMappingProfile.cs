using AutoMapper;
using IEEEBackend.Models;
using IEEEBackend.Dtos;

namespace IEEEBackend.Mappers;

public class CommitteeMappingProfile : Profile
{
    public CommitteeMappingProfile()
    {
        CreateMap<Models.Committee, CommitteeDto>()
            .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => 
                src.LogoUrl != null ? $"/{src.LogoUrl}" : null));
        CreateMap<CommitteeCreateDto, Models.Committee>();
    }
}