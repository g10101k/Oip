namespace Oip.Base.Settings;

/// <summary>
/// Front security settings
/// </summary>
public class FrontSecuritySettings
{
    public string Authority { get; set; }
    public string ClientId { get; set; }
    public string Scope { get; set; }
    public string ResponseType { get; set; }
    public bool SilentRenew { get; set; }
    public bool UseRefreshToken { get; set; }
    public int LogLevel { get; set; }
}