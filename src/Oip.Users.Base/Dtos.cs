namespace Oip.Users.Base;

/// <summary>
/// Domain representation of a user
/// </summary>
public record UserDto(
    int UserId,
    string KeycloakId,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastSyncedAt,
    byte[]? Photo,
    string Settings,
    string Phone = ""
);

/// <summary>
/// Paged result containing a list of users
/// </summary>
public record UserPagedResult(IEnumerable<UserDto> Users, int TotalCount, int PageNumber, int PageSize);