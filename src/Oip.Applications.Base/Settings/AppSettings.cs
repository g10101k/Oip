using Oip.Base.Settings;
using Oip.Settings;

namespace Oip.Applications.Settings;

/// <summary>
/// Application registry service settings.
/// </summary>
public class AppSettings : BaseAppSettings<AppSettings>, IBaseOipModuleAppSettings
{
    /// <inheritdoc />
    public OipServicesSettings Services { get; set; } = new();

    /// <inheritdoc />
    public ApplicationSettings Application { get; set; } = new()
    {
        Code = "oip-applications",
        DisplayName = "OIP.Applications",
        BaseUrl = "https://localhost:50002",
        ApiBaseUrl = "https://localhost:5008",
        Icon = "pi pi-th-large",
        Order = 30,
        Enabled = false
    };

    /// <inheritdoc />
    public ApplicationRegistrySettings ApplicationRegistry { get; set; } = new()
    {
        CurrentApplicationCode = "oip-applications"
    };

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
}