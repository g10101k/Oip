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
    public string Scope { get; set; } = null!;
}
