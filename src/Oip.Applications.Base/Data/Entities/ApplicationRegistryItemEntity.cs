using Oip.Base.Settings;

namespace Oip.Applications.Base.Data.Entities;

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
    /// Internal base URL.
    /// </summary>
    public string InternalBaseUrl { get; set; } = string.Empty;

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

    /// <summary>
    /// Type of service.
    /// </summary>
    public ServiceType ServiceType { get; set; } = ServiceType.Service;
}
