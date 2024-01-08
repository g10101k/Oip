namespace Oip.Base.Api;

/// <summary>
/// Module federation
/// </summary>
public class ModuleFederationDto
{
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

    /// <summary>
    /// Need for module federation export
    /// </summary>
    public string SourcePath { get; set; } = default!;
}