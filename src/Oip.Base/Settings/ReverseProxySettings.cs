namespace Oip.Base.Settings;

/// <summary>
/// Reverse proxy forwarded headers settings.
/// </summary>
public sealed class ReverseProxySettings
{
    /// <summary>
    /// Enables processing of reverse proxy forwarded headers.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Limits the number of entries in forwarded headers.
    /// </summary>
    public int ForwardLimit { get; set; } = 1;

    /// <summary>
    /// Trusts forwarded headers from all proxies.
    /// </summary>
    public bool TrustAllProxies { get; set; }

    /// <summary>
    /// Trusted proxy IP addresses.
    /// </summary>
    public List<string> KnownProxies { get; set; } = [];

    /// <summary>
    /// Trusted proxy networks in CIDR notation.
    /// </summary>
    public List<string> KnownNetworks { get; set; } = [];
}
