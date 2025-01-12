namespace Oip.Base.Settings;

/// <summary>
/// Security Service Settings
/// </summary>
public class SecurityServiceSettings
{
    /// <summary>
    /// Url
    /// </summary>
    public string BaseUrl { get; set; } = default!;

    /// <summary>
    /// Client
    /// </summary>
    public string ClientId { get; set; } = default!;

    /// <summary>
    /// Client secret
    /// </summary>
    public string ClientSecret { get; set; } = default!;

    /// <summary>
    /// Realm
    /// </summary>
    public string Realm { get; set; } = default!;
    
    /// <summary>
    /// Front settings
    /// </summary>
    public FrontSecuritySettings Front { get; set; }
}