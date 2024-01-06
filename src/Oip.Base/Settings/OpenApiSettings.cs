namespace Oip.Base.Settings;

/// <summary>
/// Open api/swagger config
/// </summary>
public class OpenApiSettings
{
    /// <summary>
    /// Publish swagger
    /// </summary>
    public bool Publish { get; set; } = false;

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = "v1";

    /// <summary>
    /// Version api
    /// </summary>
    public string Version { get; set; } = "v1";

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; } = "Default title";

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = "Add block \"OpenApiSettings\" in appsettings.json";
}