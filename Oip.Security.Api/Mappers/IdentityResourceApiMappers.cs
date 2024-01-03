using AutoMapper;

namespace Oip.Security.Api.Mappers;

public static class IdentityResourceApiMappers
{
    private static IMapper Mapper { get; } =
        new MapperConfiguration(cfg => cfg.AddProfile<IdentityResourceApiMapperProfile>())
            .CreateMapper();

    public static T ToIdentityResourceApiModel<T>(this object source)
    {
        return Mapper.Map<T>(source);
    }
}