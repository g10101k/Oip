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
    public OpenApiSettings OpenApi { get; set; } = new();

    /// <inheritdoc />
    public SpaDevelopmentServerSettings SpaProxyServer { get; set; } = new();

    /// <inheritdoc />
    public SecurityServiceSettings SecurityService { get; set; } = new();

    /// <inheritdoc />
    public OpenTelemetrySettings OpenTelemetry { get; set; } = new();
}