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
    OipServicesSettings Services { get; set; }

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

    /// <summary>
    /// OpenTelemetry settings.
    /// </summary>
    [NotSaveToDb] OpenTelemetrySettings OpenTelemetry { get; set; }
}