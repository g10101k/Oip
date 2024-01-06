using AutoMapper;
using Oip.Security.Api.Dtos.IdentityResources;
using Oip.Security.Bl.Dtos.Configuration;

namespace Oip.Security.Api.Mappers;

public class IdentityResourceApiMapperProfile : Profile
{
    public IdentityResourceApiMapperProfile()
    {
        // Identity Resources
        CreateMap<IdentityResourcesDto, IdentityResourcesApiDto>(MemberList.Destination)
            .ReverseMap();

        CreateMap<IdentityResourceDto, IdentityResourceApiDto>(MemberList.Destination)
            .ReverseMap();

        // Identity Resources Properties
        CreateMap<IdentityResourcePropertiesDto, IdentityResourcePropertyApiDto>(MemberList.Destination)
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdentityResourcePropertyId))
            .ReverseMap();

        CreateMap<IdentityResourcePropertyDto, IdentityResourcePropertyApiDto>(MemberList.Destination);
        CreateMap<IdentityResourcePropertiesDto, IdentityResourcePropertiesApiDto>(MemberList.Destination);
    }
}