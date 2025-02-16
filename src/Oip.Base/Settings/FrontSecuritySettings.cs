namespace Oip.Base.Settings;

/// <summary>
/// Front security settings
/// </summary>
public class FrontSecuritySettings
{
    /// <summary>
    /// Client id
    /// </summary>
    public string ClientId { get; set; } = "oip-client";

    /// <summary>
    /// Scope
    /// </summary>
    public string Scope { get; set; }

    /// <summary>
    /// Response type
    /// </summary>
    public string ResponseType { get; set; }

    /// <summary>
    /// Silent renew
    /// </summary>
    public bool SilentRenew { get; set; }

    /// <summary>
    /// Use refresh token
    /// </summary>
    public bool UseRefreshToken { get; set; }

    /// <summary>
    /// Log level
    /// </summary>
    public int LogLevel { get; set; }

    /// <summary>
    /// Urls with auth
    /// </summary>
    public List<string> SecureRoutes { get; set; } = new();
}