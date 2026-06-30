using Oip.Base.Settings;
using Oip.Settings;

namespace Oip.Users.Base.Settings;

/// <inheritdoc cref="IBaseOipModuleAppSettings"/>
public class AppSettings : BaseAppSettings<AppSettings>, IBaseOipModuleAppSettings
{
    /// <inheritdoc />
    public OipServicesSettings Services { get; set; } = new();

    /// <inheritdoc />
    public ApplicationSettings Application { get; set; } = new()
    {
        Code = "oip-users",
        DisplayName = "OIP.Users",
        BaseUrl = "https://localhost:50002",
        InternalBaseUrl = "https://localhost:5005",
        Icon = "pi pi-users",
        Order = 40,
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
    public UserSyncOptions UserSyncOptions { get; set; } = new();
}

/// <summary>
/// Represents synchronization options for the application.
/// </summary>
public class UserSyncOptions
{
    /// <summary>
    /// Gets or sets the synchronization interval in seconds.
    /// </summary>
    public int IntervalSeconds { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets the batch size for synchronization operations.
    /// </summary>
    public int BatchSize { get; set; } = 100;
    
    /// <summary>
    /// The shared secret used to validate the X-Keycloak-Signature HMAC header.
    /// </summary>
    public string SharedSecret { get; set; } = string.Empty;
}
