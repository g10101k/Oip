using Google.Protobuf.WellKnownTypes;
using Oip.Base.Clients;
using Oip.Base.Services;
using Oip.Users.Base;
using Oip.Users.Entities;
using Oip.Users.Notifications;
using Oip.Users.Repositories;
using Oip.Users.Settings;

namespace Oip.Users.Services;

/// <summary>
/// Background service for synchronizing users periodically.
/// </summary>
public class KeycloakSyncBackgroundService(
    ILogger<UserSyncService> logger,
    IServiceScopeFactory scopeFactory)
    : PeriodicBackgroundService<UserSyncService>(scopeFactory, logger);

/// <summary>
/// Service responsible for synchronizing user data between Keycloak and the local database.
/// </summary>
public class UserSyncService(
    KeycloakService keycloakService,
    UserRepository userRepository,
    UserService userService,
    BaseNotificationService notificationServiceClient,
    ILogger<UserSyncService> logger) : IPeriodicalService
{
    /// <inheritdoc />
    public int Interval => AppSettings.Instance.SyncOptions.IntervalSeconds;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await SyncAllUsersAsync();
    }

    private readonly SyncOptions _syncOptions = AppSettings.Instance.SyncOptions;

    /// <summary>
    /// Synchronizes a user from Keycloak to the local database.
    /// </summary>
    /// <param name="keycloakUserId">The ID of the user in Keycloak to synchronize.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SyncUserFromKeycloak(string keycloakUserId)
    {
        try
        {
            logger.LogInformation("Starting sync for user {KeycloakUserId}", keycloakUserId);

            var keycloakUser = await keycloakService.GetUserAsync(keycloakUserId);
            await SyncUserFromKeycloak(keycloakUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error syncing user {KeycloakUserId}", keycloakUserId);
            throw;
        }
    }

    /// <summary>
    /// Synchronizes user data from Keycloak into the local database.
    /// </summary>
    /// <param name="user">The user representation retrieved from Keycloak.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task SyncUserFromKeycloak(UserRepresentation? user)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(user.Id);
        var existingUser = await userRepository.GetByKeycloakIdAsync(user.Id);

        if (existingUser == null)
        {
            // Create new user
            var newUser = new UserEntity
            {
                KeycloakId = user.Id!,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                IsActive = user.Enabled ?? true,
                Settings = string.Empty
            };

            await userRepository.AddAsync(newUser);
            await userService.PublishUserEvent(new UserChangeEvent()
            {
                User = new User()
                {
                    KeycloakId = newUser.KeycloakId,
                    Email = newUser.Email,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    UserId = newUser.UserId,
                    IsActive = newUser.IsActive,
                    CreatedAt = newUser.CreatedAt.ToTimestamp(),
                    UpdatedAt = newUser.UpdatedAt.ToTimestamp(),
                    LastSyncedAt = newUser.LastSyncedAt.ToTimestamp(),
                },
                EventTime = DateTimeOffset.Now.ToTimestamp(),
                EventType = EventType.UserCreated
            });
            logger.LogInformation("Created new user for Keycloak ID {KeycloakUserId}", user.Id);
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
            await userRepository.UpdateAsync(existingUser);
            await userService.PublishUserEvent(new UserChangeEvent()
            {
                User = new User()
                {
                    KeycloakId = existingUser.KeycloakId,
                    Email = existingUser.Email,
                    FirstName = existingUser.FirstName,
                    LastName = existingUser.LastName,
                    UserId = existingUser.UserId,
                    IsActive = existingUser.IsActive,
                    CreatedAt = existingUser.CreatedAt.ToTimestamp(),
                    UpdatedAt = existingUser.UpdatedAt.ToTimestamp(),
                    LastSyncedAt = existingUser.LastSyncedAt.ToTimestamp(),
                },
                EventTime = DateTimeOffset.Now.ToTimestamp(),
                EventType = EventType.UserCreated
            });
            logger.LogInformation("Updated user for Keycloak ID {KeycloakUserId}", user.Id);
        }
    }


    /// <summary>
    /// Synchronizes all users from Keycloak to the local database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SyncAllUsersAsync()
    {
        logger.LogInformation("Starting full user synchronization");
        var startTime = DateTimeOffset.UtcNow;
        var totalUsers = await keycloakService.GetUsersCountAsync();
        var batches = (int)Math.Ceiling((double)totalUsers / AppSettings.Instance.SyncOptions.BatchSize);

        logger.LogInformation("Synchronizing {TotalUsers} users in {Batches} batches", totalUsers, batches);

        for (int i = 0; i < batches; i++)
        {
            var offset = i * _syncOptions.BatchSize;
            var syncedCount = await SyncUsersBatchAsync(offset, _syncOptions.BatchSize);
            logger.LogInformation("Batch {BatchNumber}: Synced {SyncedCount} users", i + 1, syncedCount);

            // Small delay to avoid overwhelming Keycloak
            await Task.Delay(1000);
        }

        var endTime = DateTimeOffset.UtcNow;
        logger.LogInformation("Full user synchronization completed");
        await notificationServiceClient.Notify(new SyncUsersCompleteNotify(totalUsers, startTime, endTime));
    }

    /// <summary>
    /// Synchronizes a batch of users from Keycloak to the local database.
    /// </summary>
    /// <param name="offset">The offset for retrieving users from Keycloak.</param>
    /// <param name="limit">The maximum number of users to retrieve in this batch.</param>
    /// <returns>The number of users successfully synchronized in this batch.</returns>
    private async Task<int> SyncUsersBatchAsync(int offset = 0, int limit = 100)
    {
        var keycloakUsers = await keycloakService.GetUsersAsync(offset, limit);
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
                logger.LogError(ex, "Error syncing user {KeycloakUserId}", keycloakUser.Id);
            }
        }

        return syncedCount;
    }
}