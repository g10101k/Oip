namespace Oip.Users.Base.Controllers.Api;

/// <summary>
/// Page of the users list.
/// </summary>
/// <param name="Users">Users of the page.</param>
/// <param name="TotalCount">Number of users matching the search.</param>
public record UsersPageDto(List<UserListItemDto> Users, int TotalCount);

/// <summary>
/// User synchronized from Keycloak.
/// </summary>
/// <param name="UserId">User id.</param>
/// <param name="Email">E-mail.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="IsActive">Whether the user is active.</param>
/// <param name="LastSyncedAt">When the user was last changed by a synchronization.</param>
public record UserListItemDto(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTimeOffset LastSyncedAt);
