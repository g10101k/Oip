using Oip.Base.Api;

namespace Oip.Base.Settings;

/// <summary>
/// ModuleFederation export settings
/// </summary>
public class ModuleFederationSettings
{
    /// <summary>
    /// ModuleFederation name
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Remote entry
    /// </summary>
    public string? RemoteEntry { get; set; }

    /// <summary>
    /// Base Url
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Export modules
    /// </summary>
    public List<ModuleFederationDto> ExportModules { get; set; } = new();

    /// <summary>
    /// Registry time out (default: 60000 milliseconds) 
    /// </summary>
    public int RegistryTimeOut { get; set; } = 60000;
}