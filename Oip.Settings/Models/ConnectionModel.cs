using Oip.Settings.Enums;

namespace Oip.Settings.Models;

/// <summary>
/// Connection model for connection string as DevExpress
/// </summary>
public class ConnectionModel
{
    /// <summary>
    /// Provider
    /// </summary>
    public XpoProvider Provider { get; set; }

    /// <summary>
    /// Connection string
    /// </summary>
    public string NormalizeConnectionString { get; set; } = default!;
}