using Oip.Base.Settings;

namespace Oip.Settings;

/// <summary>
/// Application settings
/// </summary>
public class AppSettings : BaseAppSettings<AppSettings>, IBaseOipModuleAppSettings
{
    /// <inheritdoc />
    public string OipUrls { get; set; } = null!;

    /// <inheritdoc />
    public OpenApiSettings OpenApi { get; set; } = new();

    /// <inheritdoc />
    public SpaDevelopmentServerSettings SpaProxyServer { get; set; } = new();

    /// <inheritdoc />
    public OpenTelemetrySettings Telemetry { get; set; } = new();

    /// <inheritdoc />
    public SecurityServiceSettings SecurityService { get; set; } = new();

    /// <inheritdoc />
    public List<ApiGenerationSettings> ApiGenerationSettings { get; set; } = new();
}