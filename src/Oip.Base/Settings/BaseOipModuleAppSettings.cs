using Oip.Settings;

namespace Oip.Base.Settings;

/// <inheritdoc cref="IBaseOipModuleAppSettings"/>
public class BaseOipModuleAppSettings : BaseAppSettings<BaseOipModuleAppSettings>, IBaseOipModuleAppSettings
{
    /// <inheritdoc />
    public string OipUrls { get; set; } = default!;

    /// <inheritdoc />
    public OpenApiSettings OpenApi { get; set; } = new();

    /// <inheritdoc />
    public SpaDevelopmentServerSettings SpaProxyServer { get; set; } = new();

    /// <inheritdoc />
    public ModuleFederationSettings ModuleFederation { get; set; } = new();

    /// <inheritdoc />
    public OpenTelemetrySettings Telemetry { get; set; } = new();
}