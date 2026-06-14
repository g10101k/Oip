namespace Oip.Api.Controllers.Api;

/// <summary>
/// Data transfer object representing a module and its loaded status.
/// </summary>
public class ExistModuleDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the module.
    /// </summary>
    public int ModuleId { get; set; }

    /// <summary>
    /// Gets or sets the name of the module.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether the module is currently loaded in the application.
    /// </summary>
    public bool CurrentlyLoaded { get; set; }
}