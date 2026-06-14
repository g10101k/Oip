namespace Oip.Base.Services;

/// <summary>
/// Cached user contact information used by modules that only need user identity and delivery details.
/// </summary>
public record UserCacheDto(
    int UserId,
    string KeycloakId,
    string Email,
    string? Phone);

/// <summary>
/// Provides cached user contact information without coupling modules to the users implementation.
/// </summary>
public interface IUserCacheRepository
{
    /// <summary>
    /// Users keyed by the OIP user identifier.
    /// </summary>
    IReadOnlyDictionary<int, UserCacheDto> Users { get; }

    /// <summary>
    /// Gets a cached user by Keycloak user identifier.
    /// </summary>
    UserCacheDto? GetUserByKeycloakUserId(string key);
}
