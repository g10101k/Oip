namespace Oip.Users.Base;

/// <summary>
/// Service for managing and retrieving user information in a transport-agnostic way
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves all users with pagination
    /// </summary>
    Task<UserPagedResult> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves a user by their unique identifier
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves a user by their Keycloak identifier
    /// </summary>
    Task<UserDto?> GetUserByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by their e-mail
    /// </summary>
    Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves users by identifiers
    /// </summary>
    Task<IReadOnlyList<UserDto>> GetUsersByIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches users by a free-form term
    /// </summary>
    Task<IReadOnlyList<UserDto>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default);
}
