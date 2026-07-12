using Oip.Base.Services;
using Oip.Users.Base.Data.Repositories;

namespace Oip.Users.Base.Services;

/// <summary>
/// Local implementation of IUserService that accesses the database directly.
/// Used by standalone applications and the distributed users service.
/// </summary>
public class LocalUserService(UserRepository userRepository) : IUserService
{
    /// <inheritdoc />
    public async Task<UserPagedResult> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await userRepository.GetAllUsersAsync(pageNumber, pageSize);
        
        var users = result.Results.Select(x => x.ToDto()).ToList();
        
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
        return user?.ToDto();
    }

    /// <inheritdoc />
    public async Task<UserDto?> GetUserByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByKeycloakIdAsync(keycloakId);
        return user?.ToDto();
    }

    /// <inheritdoc />
    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        return user?.ToDto();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserDto>> GetUsersByIdsAsync(
        IEnumerable<int> userIds,
        CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetByIdsAsync(userIds, cancellationToken);
        return users.Select(x => x.ToDto()).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserDto>> SearchUsersAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var users = await userRepository.SearchAsync(searchTerm);
        return users.Select(x => x.ToDto()).ToList();
    }
}
