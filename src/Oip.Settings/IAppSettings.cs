using Oip.Settings.Enums;

namespace Oip.Settings;

/// <summary>
/// Interface for base application settings class
/// </summary>
public interface IAppSettings
{
    /// <summary>
    /// Connection string in DevExpress format
    /// </summary>
    string ConnectionString { get; set; }

    /// <summary>
    /// Connection string without XpoProvider
    /// </summary>
    string NormalizedConnectionString { get; set; }

    /// <summary>
    /// Provider
    /// </summary>
    XpoProvider Provider { get; set; }

    /// <summary>
    /// Options to config application settings
    /// </summary>
    AppSettingsOptions AppSettingsOptions { get; }
}