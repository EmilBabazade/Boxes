using AutoMapper;
using Data.DTOs;
using Data.Entities;

namespace Data.AutomapperProfiles;
public class itemMapperProfile : Profile
{
    public itemMapperProfile()
    {
        CreateMap<ItemDTO, ItemEntity>().ReverseMap();
    }
}
