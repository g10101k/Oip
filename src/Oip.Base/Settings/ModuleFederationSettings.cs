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
    /// Export modules
    /// </summary>
    public ModuleFederationDto ExportModule { get; set; } = new();

    /// <summary>
    /// Registry time out (default: 60000 milliseconds) 
    /// </summary>
    public int RegistryTimeOut { get; set; } = 60000;
}