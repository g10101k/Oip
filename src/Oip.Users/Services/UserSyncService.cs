using Oip.Users.Entities;
using Oip.Users.Repositories;
using Oip.Users.Settings;
using UserRepresentation = Oip.Users.Clients.UserRepresentation;

namespace Oip.Users.Services;

/// <summary>
/// Service responsible for synchronizing user data between Keycloak and the local database.
/// </summary>
public class UserSyncService
{
    private readonly KeycloakService _keycloakService;
    private readonly UserRepository _userRepository;
    private readonly ILogger<UserSyncService> _logger;
    private readonly SyncOptions _syncOptions = AppSettings.Instance.SyncOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSyncService"/> class.
    /// </summary>
    /// <param name="keycloakService">The Keycloak service used to retrieve user data.</param>
    /// <param name="userRepository">The repository for managing user entities in the database.</param>
    /// <param name="logger">The logger for recording service operations and errors.</param>
    public UserSyncService(KeycloakService keycloakService, UserRepository userRepository,
        ILogger<UserSyncService> logger)
    {
        _keycloakService = keycloakService;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Synchronizes a user from Keycloak to the local database.
    /// </summary>
    /// <param name="keycloakUserId">The ID of the user in Keycloak to synchronize.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SyncUserFromKeycloak(string keycloakUserId)
    {
        try
        {
            _logger.LogInformation("Starting sync for user {KeycloakUserId}", keycloakUserId);

            var keycloakUser = await _keycloakService.GetUserAsync(keycloakUserId);
            await SyncUserFromKeycloak(keycloakUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing user {KeycloakUserId}", keycloakUserId);
            throw;
        }
    }

    /// <summary>
    /// Synchronizes user data from Keycloak into the local database.
    /// </summary>
    /// <param name="user">The user representation retrieved from Keycloak.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SyncUserFromKeycloak(UserRepresentation? user)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(user.Id);
        var existingUser = await _userRepository.GetByKeycloakIdAsync(user.Id);

        if (existingUser == null)
        {
            // Create new user
            var newUser = new UserEntity
            {
                KeycloakId = user.Id!,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                IsActive = user.Enabled ?? true
            };

            await _userRepository.AddAsync(newUser);
            _logger.LogInformation("Created new user for Keycloak ID {KeycloakUserId}", user.Id);
        }
        else
        {
            // Update existing user
            existingUser.Email = user.Email ?? string.Empty;
            existingUser.FirstName = user.FirstName ?? string.Empty;
            existingUser.LastName = user.LastName ?? string.Empty;
            existingUser.IsActive = user.Enabled ?? false;
            existingUser.UpdatedAt = DateTimeOffset.UtcNow;
            existingUser.LastSyncedAt = DateTimeOffset.UtcNow;
            await _userRepository.UpdateAsync(existingUser);
            _logger.LogInformation("Updated user for Keycloak ID {KeycloakUserId}", user.Id);
        }
    }


    /// <summary>
    /// Synchronizes all users from Keycloak to the local database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SyncAllUsersAsync()
    {
        _logger.LogInformation("Starting full user synchronization");

        var totalUsers = await _keycloakService.GetUsersCountAsync();
        var batches = (int)Math.Ceiling((double)totalUsers / AppSettings.Instance.SyncOptions.BatchSize);

        _logger.LogInformation("Synchronizing {TotalUsers} users in {Batches} batches", totalUsers, batches);

        for (int i = 0; i < batches; i++)
        {
            var offset = i * _syncOptions.BatchSize;
            var syncedCount = await SyncUsersBatchAsync(offset, _syncOptions.BatchSize);
            _logger.LogInformation("Batch {BatchNumber}: Synced {SyncedCount} users", i + 1, syncedCount);

            // Small delay to avoid overwhelming Keycloak
            await Task.Delay(1000);
        }

        _logger.LogInformation("Full user synchronization completed");
    }

    /// <summary>
    /// Synchronizes a batch of users from Keycloak to the local database.
    /// </summary>
    /// <param name="offset">The offset for retrieving users from Keycloak.</param>
    /// <param name="limit">The maximum number of users to retrieve in this batch.</param>
    /// <returns>The number of users successfully synchronized in this batch.</returns>
    public async Task<int> SyncUsersBatchAsync(int offset = 0, int limit = 100)
    {
        var keycloakUsers = await _keycloakService.GetUsersAsync(offset, limit);
        var syncedCount = 0;

        foreach (var keycloakUser in keycloakUsers)
        {
            if (keycloakUser.Id == null) continue;

            try
            {
                await SyncUserFromKeycloak(keycloakUser);
                syncedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing user {KeycloakUserId}", keycloakUser.Id);
            }
        }

        return syncedCount;
    }
}