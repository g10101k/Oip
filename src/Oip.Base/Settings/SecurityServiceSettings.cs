namespace Oip.Base.Settings;

/// <summary>
/// Security Service Settings
/// </summary>
public class SecurityServiceSettings
{
    /// <summary>
    /// Public Url for client
    /// </summary>
    public string BaseUrl { get; set; } = default!;

    /// <summary>
    /// Internal Url for KeycloakClient if app run in docker container
    /// </summary>
    public string? DockerUrl { get; set; }

    /// <summary>
    /// Client
    /// </summary>
    public string ClientId { get; set; } = default!;

    /// <summary>
    /// Client secret
    /// </summary>
    public string ClientSecret { get; set; } = default!;
    
    /// <summary>
    /// Gets or sets the Keycloak admin username.
    /// </summary>
    public string AdminUsername { get; set; } = default!;

    /// <summary>
    /// Gets or sets the Keycloak admin password.
    /// </summary>
    public string AdminPassword { get; set; } = default!;

    /// <summary>
    /// Realm
    /// </summary>
    public string Realm { get; set; } = default!;

    /// <summary>
    /// The number of seconds to allow for clock skew when validating tokens. Default value = 5
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 5;

    /// <summary>
    /// Front settings
    /// </summary>
    public FrontSecuritySettings Front { get; set; }
}