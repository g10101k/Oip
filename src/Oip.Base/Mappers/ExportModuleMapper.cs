using Riok.Mapperly.Abstractions;

namespace Oip.Base.Mappers;

/// <summary>
/// Mapper for ExportModule
/// </summary>
[Mapper]
public static partial class ExportModuleMapper
{
    /// <summary>
    /// To client dto
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static partial Oip.Base.Clients.ModuleFederationDto ToClientDto(this Oip.Base.Api.ModuleFederationDto model);
}