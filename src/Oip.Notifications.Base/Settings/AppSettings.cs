using Oip.Base.Settings;
using Oip.Settings;

namespace Oip.Notifications.Base.Settings;

/// <inheritdoc cref="IBaseOipModuleAppSettings"/>
public class AppSettings : BaseAppSettings<AppSettings>, IBaseOipModuleAppSettings
{
    /// <inheritdoc />
    public OipServicesSettings Services { get; set; } = new();

    /// <inheritdoc />
    public ApplicationSettings Application { get; set; } = new()
    {
        Code = "oip-notifications",
        DisplayName = "OIP.Notifications",
        BaseUrl = "https://localhost:50002",
        InternalBaseUrl = "https://localhost:5007",
        Icon = "pi pi-bell",
        Order = 50,
        Enabled = false,
        ServiceType = ServiceType.Service
    };

    /// <inheritdoc />
    public ApplicationRegistrySettings ApplicationRegistry { get; set; } = new();

    /// <inheritdoc />
    public OpenApiSettings OpenApi { get; set; } = new();

    /// <inheritdoc />
    public SpaDevelopmentServerSettings SpaProxyServer { get; set; } = new();

    /// <inheritdoc />
    public SecurityServiceSettings SecurityService { get; set; } = new();

    /// <inheritdoc />
    public OpenTelemetrySettings OpenTelemetry { get; set; } = new();

    /// <inheritdoc />
    public bool IsStandalone { get; set; } = false;

    /// <inheritdoc />
    public DataProtectionSettings DataProtection { get; set; } = new();

    /// <inheritdoc />
    public ReverseProxySettings ReverseProxy { get; set; } = new();

    /// <inheritdoc />
    public bool GenerateWebClient { get; set; }

    /// <summary>
    /// Represents synchronization options for the application.
    /// </summary>
    public SyncOptions SyncOptions { get; set; } = new();
    
    public CorsSettings Cors { get; set; } = new();
}

/// <summary>
/// Represents synchronization options for the application.
/// </summary>
public class SyncOptions
{
    /// <summary>
    /// Gets or sets the synchronization interval in minutes.
    /// </summary>
    public int IntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the batch size for synchronization operations.
    /// </summary>
    public int BatchSize { get; set; } = 100;
}