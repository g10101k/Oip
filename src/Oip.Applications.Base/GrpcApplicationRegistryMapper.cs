using Oip.Applications.Base.Contracts;
using Oip.Applications.Base.Grpc;
using Riok.Mapperly.Abstractions;

namespace Oip.Applications.Base;

/// <summary>
/// Maps application registry contracts between DTO and gRPC shapes.
/// </summary>
[Mapper]
public static partial class GrpcApplicationRegistryMapper
{
    public static partial ApplicationRegistryItemDto ToDto(this ApplicationRegistryItem application);

    public static partial ApplicationRegistryItem ToGrpc(this ApplicationRegistryItemDto application);
}
