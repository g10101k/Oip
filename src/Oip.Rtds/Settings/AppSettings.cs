using Oip.Base.Settings;
using Oip.Rtds.Data;
using Oip.Settings;

namespace Oip.Rtds.Settings;

/// <inheritdoc cref="IBaseOipModuleAppSettings"/>
public class AppSettings : BaseAppSettings<AppSettings>, IBaseOipModuleAppSettings, IRtdsAppSettings
{
    /// <summary>
    /// Gets or sets the connection string for the RTD service
    /// </summary>
    public string RtsConnectionString { get; set; } = null!;

    /// <inheritdoc />
    public OipServicesSettings Services { get; set; } = new();

    /// <inheritdoc />
    public ApplicationSettings Application { get; set; } = new()
    {
        Code = "oip-rtds",
        DisplayName = "OIP.Rtds",
        BaseUrl = "https://localhost:50003",
        InternalBaseUrl = "https://localhost:5003",
        Icon = "pi pi-database",
        Order = 20,
        ServiceType = ServiceType.Application
    };

    /// <inheritdoc />
    public ApplicationRegistrySettings ApplicationRegistry { get; set; } =
        new() { CurrentApplicationCode = "oip-rtds" };

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
}
