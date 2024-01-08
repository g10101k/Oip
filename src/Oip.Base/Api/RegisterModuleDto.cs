namespace Oip.Base.Api;

/// <summary>
/// Dto module
/// </summary>
public class RegisterModuleDto
{
    /// <summary>
    /// See 'name' in webpack.config.js
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Base Url
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Remote entry
    /// </summary>
    public string? RemoteEntry { get; set; }
    
    /// <summary>
    /// Module federation
    /// </summary>
    public List<ModuleFederationDto> ExportModules { get; set; } = new();
}