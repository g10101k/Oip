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
    AddingMode ServiceAddingMode { get; set; }

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

public enum AddingMode
{
    /// <summary>
    /// The service is added locally:
    /// - the data layer is added
    /// - the business logic layer is added
    /// - all controllers are connected without restrictions (AddController is not used)
    /// - internal gRPC is not started.
    /// </summary>
    Local = 0,
    /// <summary>
    /// Mode for adding remote services; a gRPC client is connected for remote service calls.
    /// Caching services are added.
    /// </summary>
    Remote = 1,
    /// <summary>
    /// The service is added locally for remote use.
    /// - the data layer is added
    /// - the business logic layer is added
    /// - controllers are connected through AddController (only controllers added through AddController are active)
    /// - gRPC is started.
    /// </summary>
    Service = 2
}
