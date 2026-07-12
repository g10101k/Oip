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
    AddingMode AddingMode { get; set; }

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
    /// Сервис добавляется локально:
    /// - добавляется слой данных
    /// - слой логики,
    /// - подключаются все контроллеры без ограниченйи (AddController не используется),
    /// - gRPC для внутреннего не поднимается.
    /// </summary>
    Local = 0,
    /// <summary>
    /// Режим добавления удаленных сервисов, подключается gRPC клиент для удаленного вызова сервиса.
    /// Добавляются сервисы кеширования.
    /// </summary>
    Remote = 1,
    /// <summary>
    /// Сервис добавляется локально для удаленного использования.
    /// - добавляется слой данных
    /// - слой логики,
    /// - подключаются  контроллеры через AddController (Работают только контроллеры добавленные через AddController)
    /// - gRPC поднимается.
    /// </summary>
    Service = 2
}