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
    /// Module federation
    /// </summary>
    public ModuleFederationDto ExportModule { get; set; } = new();
}