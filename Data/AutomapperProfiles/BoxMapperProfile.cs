using AutoMapper;
using Data.DTOs;
using Data.Entities;

namespace Data.AutomapperProfiles;
public class BoxMapperProfile : Profile
{
    public BoxMapperProfile()
    {
        CreateMap<BoxDTO, BoxEntity>().ReverseMap();
    }
}
