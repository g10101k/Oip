namespace Oip.Rtds.Data;

/// <summary>
/// Defines the interface for RTDS application settings.
/// </summary>
public interface IRtdsAppSettings
{
    /// <summary>
    /// The connection string used to connect to the RTDS ClickHouse database.
    /// </summary>
    public string RtsConnectionString { get; set; }
}