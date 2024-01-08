namespace Oip.Controllers;

/// <summary>
/// Response for module federation
/// </summary>
public class GetManifestResponse
{
    /// <summary>
    /// Remote entry
    /// </summary>
    public string? RemoteEntry { get; set; }

    /// <summary>
    /// Base Url
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Module name see exposes in webpack.config.js
    /// </summary>
    public string ExposedModule { get; set; } = default!;

    /// <summary>
    /// Display name 
    /// </summary>
    public string DisplayName { get; set; } = default!;

    /// <summary>
    /// Route
    /// </summary>
    public string RoutePath { get; set; } = default!;

    /// <summary>
    /// NgModuleName
    /// </summary>
    public string NgModuleName { get; set; } = default!;
}