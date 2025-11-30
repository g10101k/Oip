using Oip.Settings;

namespace Oip.Base.Settings;

/// <summary>
/// Base settings for module application
/// </summary>
public interface IBaseOipModuleAppSettings : IAppSettings
{
    /// <summary>
    /// Main service with shell
    /// </summary>
    string OipUrls { get; set; }

    /// <summary>
    /// Collection of OpenAPI specification configurations for the application
    /// </summary>
    OpenApiSettings OpenApi { get; set; }

    /// <summary>
    /// Spa proxy server settings
    /// </summary>
    SpaDevelopmentServerSettings SpaProxyServer { get; set; }

    /// <summary>
    /// Security Service Settings
    /// </summary>
    SecurityServiceSettings SecurityService { get; set; }
}

/// <summary>
/// Contains OpenAPI/Swagger configuration items for the application
/// </summary>
public class OpenApiSettings : List<OpenApiItem>;