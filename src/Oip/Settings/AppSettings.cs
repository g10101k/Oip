using Oip.Base.Settings;
using Oip.Users.Base.Settings;

namespace Oip.Settings;

/// <summary>
/// Application settings
/// </summary>
public class AppSettings : BaseAppSettings<AppSettings>, IBaseOipModuleAppSettings
{
    /// <inheritdoc />
    public OipServicesSettings Services { get; set; } = new();

    /// <inheritdoc />
    public ApplicationSettings Application { get; set; } = new()
    {
        Code = "oip",
        DisplayName = "OIP",
        BaseUrl = "https://localhost:50002",
        ApiBaseUrl = "https://localhost:5002",
        Icon = "pi pi-home",
        Order = 10
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

    /// <summary>
    /// User synchronization option
    /// </summary>
    public UserSyncOptions UserSyncOptions { get; set; } = new();
}
