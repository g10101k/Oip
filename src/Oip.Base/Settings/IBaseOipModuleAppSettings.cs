using Oip.Settings;
using Oip.Settings.Attributes;

namespace Oip.Base.Settings;

/// <summary>
/// Base settings for module application
/// </summary>
public interface IBaseOipModuleAppSettings : IAppSettings
{
    /// <summary>
    /// Collection of urls OIP services
    /// </summary>
    OipServices Services { get; set; }

    /// <summary>
    /// Collection of OpenAPI specification configurations for the application
    /// </summary>
    [NotSaveToDb]
    OpenApiSettings OpenApi { get; set; }

    /// <summary>
    /// Spa proxy server settings
    /// </summary>
    [NotSaveToDb]
    SpaDevelopmentServerSettings SpaProxyServer { get; set; }

    /// <summary>
    /// Security Service Settings
    /// </summary>
    [NotSaveToDb]
    SecurityServiceSettings SecurityService { get; set; }
}

/// <summary>
/// Contains OpenAPI/Swagger configuration items for the application
/// </summary>
public class OpenApiSettings : List<OpenApiItem>;

/// <summary>
/// Contains service endpoint configurations for OIP app
/// </summary>
public class OipServices
{
    /// <summary>
    /// URL endpoint for the OIP users service
    /// </summary>
    public string OipUsers { get; set; } = "https://localhost:5005";

    /// <summary>
    /// Gets or sets the endpoint URL for the OIP notifications service
    /// </summary>
    public string OipNotifications { get; set; } = "https://localhost:5007";
}