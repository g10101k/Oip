using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Oip.Base.Services;

/// <summary>
/// Reads and writes session metadata kept inside an authentication ticket.
/// </summary>
public static class AuthSessionMetadata
{
    /// <summary>
    /// Claim carrying the opaque ticket store key of the session the principal was loaded from.
    /// </summary>
    public const string SessionKeyClaimType = "oip:session_key";

    private const string CreatedUtcKey = "oip.session.created";
    private const string LastActivityUtcKey = "oip.session.lastActivity";
    private const string IpAddressKey = "oip.session.ip";
    private const string UserAgentKey = "oip.session.userAgent";
    private const int MaxUserAgentLength = 512;

    /// <summary>
    /// Minimal delay between two last activity updates written to the store.
    /// </summary>
    public static readonly TimeSpan ActivityUpdateInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Attaches the store key to the ticket principal and records where the session was created from.
    /// </summary>
    public static void Initialize(string key, AuthenticationTicket ticket, HttpContext? httpContext, DateTimeOffset now)
    {
        if (ticket.Principal.Identity is ClaimsIdentity identity && !identity.HasClaim(c => c.Type == SessionKeyClaimType))
            identity.AddClaim(new Claim(SessionKeyClaimType, key));

        var items = ticket.Properties.Items;
        if (!items.ContainsKey(CreatedUtcKey))
            items[CreatedUtcKey] = now.ToString("O");
        if (!items.ContainsKey(LastActivityUtcKey))
            items[LastActivityUtcKey] = now.ToString("O");

        if (httpContext is null)
            return;

        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(ipAddress) && !items.ContainsKey(IpAddressKey))
            items[IpAddressKey] = ipAddress;

        var userAgent = httpContext.Request.Headers.UserAgent.FirstOrDefault();
        if (!string.IsNullOrEmpty(userAgent) && !items.ContainsKey(UserAgentKey))
            items[UserAgentKey] = userAgent.Length > MaxUserAgentLength ? userAgent[..MaxUserAgentLength] : userAgent;
    }

    /// <summary>
    /// Updates the last activity stamp when <see cref="ActivityUpdateInterval"/> has elapsed.
    /// </summary>
    /// <returns><c>true</c> when the ticket changed and should be persisted again.</returns>
    public static bool TouchActivity(AuthenticationTicket ticket, DateTimeOffset now)
    {
        var lastActivity = GetDate(ticket.Properties, LastActivityUtcKey);
        if (lastActivity is not null && now - lastActivity.Value < ActivityUpdateInterval)
            return false;

        ticket.Properties.Items[LastActivityUtcKey] = now.ToString("O");
        return true;
    }

    /// <summary>
    /// Builds the session description from a stored ticket.
    /// </summary>
    public static AuthSessionInfo Create(string key, AuthenticationTicket ticket, DateTimeOffset? lastActivityUtc = null)
    {
        var principal = ticket.Principal;
        var properties = ticket.Properties;

        return new AuthSessionInfo(
            key,
            ComputeSessionId(key),
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub"),
            principal.FindFirstValue("preferred_username") ?? principal.Identity?.Name,
            principal.FindFirstValue("name"),
            principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email"),
            GetDate(properties, CreatedUtcKey) ?? properties.IssuedUtc,
            lastActivityUtc ?? GetDate(properties, LastActivityUtcKey) ?? properties.IssuedUtc,
            properties.ExpiresUtc,
            GetItem(properties, IpAddressKey),
            GetItem(properties, UserAgentKey));
    }

    /// <summary>
    /// Derives the public session identifier from the store key so the key itself never leaves the server.
    /// </summary>
    public static string ComputeSessionId(string key) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)))[..32].ToLowerInvariant();

    private static string? GetItem(AuthenticationProperties properties, string key) =>
        properties.Items.TryGetValue(key, out var value) ? value : null;

    private static DateTimeOffset? GetDate(AuthenticationProperties properties, string key) =>
        properties.Items.TryGetValue(key, out var value) &&
        DateTimeOffset.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var date)
            ? date
            : null;
}
