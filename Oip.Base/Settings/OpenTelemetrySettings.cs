namespace Oip.Base.Settings;

/// <summary>
/// Open Telemetry Settings
/// </summary>
public class OpenTelemetrySettings
{
    /// <summary>
    /// Use Prometheus exporter default - false
    /// </summary>
    public bool UsePrometheusExporter { get; set; } = false;
}