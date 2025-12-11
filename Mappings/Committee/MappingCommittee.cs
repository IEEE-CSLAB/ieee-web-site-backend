using AutoMapper;
using IEEEBackend.Models;
using IEEEBackend.DTOs;

namespace IEEEBackend.Mappings.Committee;

public class MappingCommittee : Profile
{
    public MappingCommittee()
    {
        CreateMap<Models.Committee, CommitteeDto>();
        CreateMap<CommitteeCreateDto, Models.Committee>();
    }
}