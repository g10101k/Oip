using Oip.Settings;
using Oip.Settings.Attributes;

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