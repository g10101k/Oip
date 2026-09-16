namespace Oip.Base.Controllers.Api;

/// <summary>
/// An active authentication session shown to administrators.
/// </summary>
public sealed class AuthSessionDto
{
    /// <summary>
    /// Public identifier of the session, derived from the store key.
    /// </summary>
    public required string SessionId { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public DateTimeOffset? CreatedUtc { get; set; }

    public DateTimeOffset? LastActivityUtc { get; set; }

    public DateTimeOffset? ExpiresUtc { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    /// <summary>
    /// Whether this is the session of the caller.
    /// </summary>
    public bool IsCurrent { get; set; }
}
