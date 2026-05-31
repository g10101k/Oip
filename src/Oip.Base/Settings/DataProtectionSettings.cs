namespace Oip.Base.Settings;

/// <summary>
/// Settings for data protection services in the OIP application.
/// </summary>
public class DataProtectionSettings
{
    /// <summary>
    /// The name of the application used as a unique identifier for key isolation.
    /// Default value: "OIP".
    /// </summary>
    public string ApplicationName { get; set; } = "OIP";

    /// <summary>
    /// File system path where data protection keys will be persisted. If not set, use default system path
    /// Default value: null. 
    /// </summary>
    public string? PersistKeysToFileSystemPath { get; set; } = null;

    /// <summary>
    /// Default lifetime of data protection keys in days.
    /// Default value: 365 days (1 year).
    /// </summary>
    public int DefaultKeyLifetimeInDay { get; set; } = 365;
}