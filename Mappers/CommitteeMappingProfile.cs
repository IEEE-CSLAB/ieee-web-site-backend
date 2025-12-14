using AutoMapper;
using IEEEBackend.Models;
using IEEEBackend.Dtos;

namespace IEEEBackend.Mappers;

public class CommitteeMappingProfile : Profile
{
    public CommitteeMappingProfile()
    {
        CreateMap<Models.Committee, CommitteeDto>();
        CreateMap<CommitteeCreateDto, Models.Committee>();
    }
}