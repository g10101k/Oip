namespace Oip.Controllers.Api;

/// <summary>
/// Module federation
/// </summary>
public class ModuleFederation
{
    /// <summary>
    /// Remote entry
    /// </summary>
    public string RemoteEntry { get; set; } = default!;

    /// <summary>
    /// Base Url
    /// </summary>
    public string BaseUrl { get; set; } = default!;

    /// <summary>
    /// Module name
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