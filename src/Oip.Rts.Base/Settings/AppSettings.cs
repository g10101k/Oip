using Oip.Base.Settings;
using Oip.Settings;

namespace Oip.Rts.Base.Settings;

/// <inheritdoc cref="IBaseOipModuleAppSettings"/>
public class AppSettings: BaseAppSettings<AppSettings>, IBaseOipModuleAppSettings
{
    /// <summary>
    /// 
    /// </summary>
    public string RtsConnectionString { get; set; } = null!;

    /// <inheritdoc />
    public string OipUrls { get; set; } = default!;

    /// <inheritdoc />
    public OpenApiSettings OpenApi { get; set; } = new();

    /// <inheritdoc />
    public SpaDevelopmentServerSettings SpaProxyServer { get; set; } = new();

    /// <inheritdoc />
    public OpenTelemetrySettings Telemetry { get; set; } = new();
    
    /// <inheritdoc />
    public SecurityServiceSettings SecurityService { get; set; } = new();
}