namespace Oip.Base.Services;

/// <summary>
/// Describes an active cookie authentication session held by a ticket store.
/// </summary>
public sealed record AuthSessionInfo(
    string Key,
    string SessionId,
    string? UserId,
    string? UserName,
    string? DisplayName,
    string? Email,
    DateTimeOffset? CreatedUtc,
    DateTimeOffset? LastActivityUtc,
    DateTimeOffset? ExpiresUtc,
    string? IpAddress,
    string? UserAgent);
