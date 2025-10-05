using Oip.Base.Discovery;
using Oip.Base.Settings;
using Oip.Settings;
using Oip.Users.Data;

namespace Oip.Users.Settings;

/// <inheritdoc cref="IBaseOipModuleAppSettings"/>
public class AppSettings : BaseAppSettings<AppSettings>, IBaseOipModuleAppSettings
{
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

    /// <inheritdoc />
    public ServiceDiscoverySettings ServiceDiscovery { get; set; } = new();

    /// <summary>
    /// Represents synchronization options for the application.
    /// </summary>
    public SyncOptions SyncOptions { get; set; } = new();
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