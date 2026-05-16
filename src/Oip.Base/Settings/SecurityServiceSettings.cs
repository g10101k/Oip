namespace Oip.Base.Settings;

/// <summary>
/// Security Service Settings
/// </summary>
public class SecurityServiceSettings
{
    /// <summary>
    /// Public Url for client
    /// </summary>
    public string BaseUrl { get; set; } = null!;

    /// <summary>
    /// Internal Url for KeycloakClient if app run in docker container
    /// </summary>
    public string? DockerUrl { get; set; }

    /// <summary>
    /// Client
    /// </summary>
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// Client secret
    /// </summary>
    public string ClientSecret { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the Keycloak admin username.
    /// </summary>
    public string AdminUsername { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Keycloak admin password.
    /// </summary>
    public string AdminPassword { get; set; } = null!;

    /// <summary>
    /// Realm
    /// </summary>
    public string Realm { get; set; } = null!;

    /// <summary>
    /// The number of seconds to allow for clock skew when validating tokens. Default value = 5
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 5;

    /// <summary>
    /// Front settings
    /// </summary>
    public FrontSecuritySettings Front { get; set; } = null!;

    /// <summary>
    /// Authentication ticket store settings.
    /// </summary>
    public AuthTicketStoreSettings AuthTicketStore { get; set; } = new();
}

/// <summary>
/// Authentication ticket store settings.
/// </summary>
public class AuthTicketStoreSettings
{
    /// <summary>
    /// Authentication ticket lifetime in minutes.
    /// </summary>
    public int TicketLifetimeMinutes { get; set; } = 20160;

    /// <summary>
    /// Redis connection string for distributed authentication ticket storage.
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Distributed cache key prefix for authentication tickets.
    /// </summary>
    public string DistributedKeyPrefix { get; set; } = "Oip:AuthTicket:";

    /// <summary>
    /// Maximum number of tickets kept by the in-memory fallback store.
    /// </summary>
    public int MaxInMemoryTickets { get; set; } = 10000;

    /// <summary>
    /// Minimum interval in seconds between in-memory cleanup passes.
    /// </summary>
    public int CleanupIntervalSeconds { get; set; } = 300;
}
