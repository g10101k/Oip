namespace Oip.Base.Settings;

/// <summary>
/// Contains frontend application registry configuration.
/// </summary>
public class ApplicationRegistrySettings
{
    /// <summary>
    /// Current application code.
    /// </summary>
    public string CurrentApplicationCode { get; set; } = "oip";

    /// <summary>
    /// Registered frontend applications.
    /// </summary>
    public List<ApplicationRegistryItemSettings> Applications { get; set; } = [];
}

/// <summary>
/// Contains the current service frontend application passport.
/// </summary>
public class ApplicationSettings
{
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
    /// Type of service
    /// </summary>
    public ServiceType ServiceType { get; set; } = ServiceType.Service;
}

/// <summary>
/// Contains one frontend application registration.
/// </summary>
public class ApplicationRegistryItemSettings
{
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
    /// Type of service
    /// </summary>
    public ServiceType ServiceType { get; set; } = ServiceType.Service;
}

/// <summary>
/// Type of service
/// </summary>
public enum ServiceType
{
    /// <summary>
    /// Service api only
    /// </summary>
    Service,
    /// <summary>
    /// Application with UI
    /// </summary>
    Application
}