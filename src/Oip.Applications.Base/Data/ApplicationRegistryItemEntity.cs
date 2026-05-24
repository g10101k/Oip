namespace Oip.Applications.Data;

/// <summary>
/// Persisted frontend application registry item.
/// </summary>
public class ApplicationRegistryItemEntity
{
    /// <summary>
    /// Internal database identifier.
    /// </summary>
    public long ApplicationRegistryItemId { get; set; }

    /// <summary>
    /// Stable application code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable application name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Frontend application URL.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Backend API URL.
    /// </summary>
    public string ApiBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// PrimeIcons CSS class.
    /// </summary>
    public string Icon { get; set; } = "pi pi-circle";

    /// <summary>
    /// Display order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Indicates whether application should be returned to frontend.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
