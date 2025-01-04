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
    /// OpenAPI settings
    /// </summary>
    OpenApiSettings OpenApi { get; set; }

    /// <summary>
    /// Spa proxy server settings
    /// </summary>
    SpaDevelopmentServerSettings SpaProxyServer { get; set; }

    /// <summary>
    /// Open telemetry settings
    /// </summary>
    OpenTelemetrySettings Telemetry { get; set; }

    /// <summary>
    /// Security Service Settings
    /// </summary>
    SecurityServiceSettings SecurityService { get; set; }
}