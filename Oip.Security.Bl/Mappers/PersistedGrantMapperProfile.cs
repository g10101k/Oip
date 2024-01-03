using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using Oip.Security.Bl.Dtos.Grant;
using Oip.Security.Dal.Common;
using Oip.Security.Dal.Entities;

namespace Oip.Security.Bl.Mappers;

public class PersistedGrantMapperProfile : Profile
{
    public PersistedGrantMapperProfile()
    {
        // entity to model
        CreateMap<PersistedGrant, PersistedGrantDto>(MemberList.Destination)
            .ReverseMap();

        CreateMap<PersistedGrantDataView, PersistedGrantDto>(MemberList.Destination);

        CreateMap<PagedList<PersistedGrantDataView>, PersistedGrantsDto>(MemberList.Destination)
            .ForMember(x => x.PersistedGrants,
                opt => opt.MapFrom(src => src.Data));

        CreateMap<PagedList<PersistedGrant>, PersistedGrantsDto>(MemberList.Destination)
            .ForMember(x => x.PersistedGrants,
                opt => opt.MapFrom(src => src.Data));
    }
}