using Oip.Users.Base;
using Oip.Users.Repositories;
using Oip.Users.Entities;

namespace Oip.Users.Services;

/// <summary>
/// Local implementation of IUserService that accesses the database directly.
/// Used when IsStandalone is true.
/// </summary>
public class LocalUserService(UserRepository userRepository) : IUserService
{
    /// <inheritdoc />
    public async Task<UserPagedResult> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await userRepository.GetAllUsersAsync(pageNumber, pageSize);
        
        var users = result.Results.Select(MapToDto).ToList();
        
        return new UserPagedResult(
            Users: users,
            TotalCount: result.TotalCount,
            PageNumber: pageNumber,
            PageSize: pageSize
        );
    }

    /// <inheritdoc />
    public async Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        return user == null ? null : MapToDto(user);
    }

    /// <inheritdoc />
    public async Task<UserDto?> GetUserByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByKeycloakIdAsync(keycloakId);
        return user == null ? null : MapToDto(user);
    }

    /// <inheritdoc />
    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        return user == null ? null : MapToDto(user);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserDto>> GetUsersByIdsAsync(
        IEnumerable<int> userIds,
        CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetByIdsAsync(userIds, cancellationToken);
        return users.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserDto>> SearchUsersAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var users = await userRepository.SearchAsync(searchTerm);
        return users.Select(MapToDto).ToList();
    }

    private static UserDto MapToDto(UserEntity entity)
    {
        return new UserDto(
            UserId: entity.UserId,
            KeycloakId: entity.KeycloakId,
            Email: entity.Email,
            FirstName: entity.FirstName,
            LastName: entity.LastName,
            IsActive: entity.IsActive,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt,
            LastSyncedAt: entity.LastSyncedAt,
            Photo: entity.Photo,
            Settings: entity.Settings
        );
    }
}
