using Oip.Applications.Base.Contracts;
using Oip.Applications.Base.Grpc;

namespace Oip.Applications.Base;

/// <summary>
/// Maps application registry contracts between DTO and gRPC shapes.
/// </summary>
public static class GrpcApplicationRegistryMapper
{
    public static ApplicationRegistryItemDto ToDto(this ApplicationRegistryItem application)
    {
        return new ApplicationRegistryItemDto
        {
            Code = application.Code,
            DisplayName = application.DisplayName,
            BaseUrl = application.BaseUrl,
            ApiBaseUrl = application.ApiBaseUrl,
            Icon = application.Icon,
            Order = application.Order,
            Enabled = application.Enabled,
            IsCurrent = application.IsCurrent
        };
    }

    public static ApplicationRegistryItem ToGrpc(this ApplicationRegistryItemDto application)
    {
        return new ApplicationRegistryItem
        {
            Code = application.Code,
            DisplayName = application.DisplayName,
            BaseUrl = application.BaseUrl,
            ApiBaseUrl = application.ApiBaseUrl,
            Icon = application.Icon,
            Order = application.Order,
            Enabled = application.Enabled,
            IsCurrent = application.IsCurrent
        };
    }
}
