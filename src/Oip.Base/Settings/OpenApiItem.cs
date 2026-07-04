namespace Oip.Base.Settings;

/// <summary>
/// Open api/swagger config
/// </summary>
public class OpenApiItem
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
    /// Url
    /// </summary>
    public string Url { get; set; } = "/swagger/v1/swagger.json";

    /// <summary>
    /// Version api
    /// </summary>
    public string Version { get; set; } = "v1.0.0";

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; } = "Default title";

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = "Add block \"OpenApi\" in appsettings.json";
    
    /// <summary>
    /// Command that generates a web API client from the OpenAPI document.
    /// When specified, the application generates the client during startup and stops after generation completes.
    /// </summary>
    public string? GenerateCommand { get; set; }

    /// <summary>
    /// Working directory for generating openapi specification. By default, is null and use SpaProxy config
    /// Use if service not use SPA
    /// </summary>
    public string? WorkingDirectory { get; set; } = null;
}
