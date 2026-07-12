using Oip.Settings;
using Oip.Settings.Attributes;

namespace Oip.Base.Settings;

/// <summary>
/// Base settings for instance of OIP
/// </summary>
public interface ISettings : IAppSettings
{
    /// <summary>
    /// Collection of urls OIP services
    /// </summary>
    OipServicesSettings Services { get; set; }

    /// <summary>
    /// Current service frontend application passport.
    /// </summary>
    [NotSaveToDb]
    ApplicationSettings Application { get; set; }

    /// <summary>
    /// Frontend applications registry.
    /// </summary>
    [NotSaveToDb]
    ApplicationRegistrySettings ApplicationRegistry { get; set; }

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
    [NotSaveToDb]
    OpenTelemetrySettings OpenTelemetry { get; set; }

    /// <summary>
    /// Defines how the application participates in the OIP deployment.
    /// </summary>
    [NotSaveToDb]
    StartupMode StartupMode { get; set; }

    /// <summary>
    /// DataProtection settings
    /// </summary>
    [NotSaveToDb]
    DataProtectionSettings DataProtection { get; set; }

    /// <summary>
    /// CORS Settings
    /// </summary>
    [NotSaveToDb]
    CorsSettings Cors { get; set; }

    /// <summary>
    /// Reverse proxy forwarded headers settings.
    /// </summary>
    [NotSaveToDb]
    ReverseProxySettings ReverseProxy { get; set; }

    /// <summary>
    /// Generate web client
    /// </summary>
    [NotSaveToDb]
    bool GenerateWebClient { get; set; }
}

public enum StartupMode
{
    Standalone = 0,
    Remote = 1,
    Service = 2
}
