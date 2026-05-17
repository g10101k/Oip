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
    public List<ApplicationRegistryItemSettings> Applications { get; set; } =
    [
        new()
        {
            Code = "oip",
            DisplayName = "OIP",
            BaseUrl = "https://localhost:50002",
            ApiBaseUrl = "https://localhost:5002",
            Icon = "pi pi-home",
            Order = 10
        },
        new()
        {
            Code = "oip-rtds",
            DisplayName = "OIP.Rtds",
            BaseUrl = "https://localhost:50003",
            ApiBaseUrl = "https://localhost:5003",
            Icon = "pi pi-database",
            Order = 20
        }
    ];
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
