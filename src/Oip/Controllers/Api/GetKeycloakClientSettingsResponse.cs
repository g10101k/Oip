namespace Oip.Controllers.Api;

/// <summary>
/// Response front security settings
/// </summary>
public class GetKeycloakClientSettingsResponse
{
    /// <summary>
    /// Authority
    /// </summary>
    public string Authority { get; set; } = default!;

    /// <summary>
    /// Client id
    /// </summary>
    public string ClientId { get; set; } = default!;

    /// <summary>
    /// Scope
    /// </summary>
    public string Scope { get; set; } = default!;

    /// <summary>
    /// Response Type
    /// </summary>
    public string ResponseType { get; set; } = default!;

    /// <summary>
    /// Use Refresh Token
    /// </summary>
    public bool UseRefreshToken { get; set; }

    /// <summary>
    /// Silent Renew
    /// </summary>
    public bool SilentRenew { get; set; }

    /// <summary>
    /// Log level None = 0, Debug = 1, Warn = 2, Error = 3 
    /// </summary>
    public int LogLevel { get; set; }
}