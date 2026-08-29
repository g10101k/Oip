using Oip.Base.Services;

namespace Oip.Users.Base.Data.Repositories;

/// <summary>
/// Lightweight implementation of <see cref="IUserCacheRepository"/> for Local and Service modes.
/// Queries the local user store directly on demand instead of maintaining an in-memory cache that is
/// periodically refreshed by a background service, which is only meaningful when users are synchronized
/// remotely via gRPC.
/// </summary>
public class LocalUserCacheRepository(UserRepository userRepository) : IUserCacheRepository
{
    private IReadOnlyDictionary<int, UserCacheDto>? _users;

    /// <inheritdoc />
    public IReadOnlyDictionary<int, UserCacheDto> Users => _users ??= LoadUsers();

    /// <inheritdoc />
    public UserCacheDto? GetUserByKeycloakUserId(string key)
    {
        var user = userRepository.GetByKeycloakIdAsync(key).GetAwaiter().GetResult();
        return user?.ToDto().ToCacheDto();
    }

    private IReadOnlyDictionary<int, UserCacheDto> LoadUsers()
    {
        var users = userRepository.GetActiveKeycloakUsersAsync().GetAwaiter().GetResult();
        return users.ToDictionary(u => u.UserId, u => u.ToDto().ToCacheDto());
    }
}
