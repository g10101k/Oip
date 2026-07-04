using System.Collections.Concurrent;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Oip.Base.Clients;
using Oip.Base.Services;
using Oip.Users.Base.Data.Entities;
using Oip.Users.Base.Data.Repositories;
using Oip.Users.Base.Notifications;
using Oip.Users.Base.Settings;

namespace Oip.Users.Base.Services;

/// <summary>
/// Service responsible for synchronizing user data between Keycloak and the local database.
/// </summary>
public class KeycloakSyncService(
    KeycloakService keycloakService,
    UserRepository userRepository,
    UserService userService,
    INotificationPublisher notificationPublisher,
    UserSyncOptions userSyncOptions,
    ILogger<KeycloakSyncService> logger)
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> SyncLocks =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Synchronizes a user from Keycloak to the local database.
    /// </summary>
    /// <param name="keycloakUserId">The ID of the user in Keycloak to synchronize.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SyncUserFromKeycloak(string keycloakUserId)
    {
        var syncLock = SyncLocks.GetOrAdd(keycloakUserId, _ => new SemaphoreSlim(1, 1));
        await syncLock.WaitAsync();

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
        finally
        {
            syncLock.Release();
        }
    }

    /// <summary>
    /// Marks a local user inactive after the user was removed from Keycloak.
    /// </summary>
    /// <param name="keycloakUserId">The ID of the user in Keycloak.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeactivateUserFromKeycloak(string keycloakUserId)
    {
        var existingUser = await userRepository.GetByKeycloakIdAsync(keycloakUserId);
        if (existingUser == null)
        {
            logger.LogInformation("Ignored Keycloak delete event for unknown user {KeycloakUserId}", keycloakUserId);
            return;
        }

        if (!existingUser.IsActive)
        {
            logger.LogInformation("Ignored Keycloak delete event for inactive user {KeycloakUserId}", keycloakUserId);
            return;
        }

        await DeactivateUserAsync(existingUser);

        logger.LogInformation("Deactivated user for deleted Keycloak ID {KeycloakUserId}", keycloakUserId);
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
            var email = user.Email ?? string.Empty;
            var firstName = user.FirstName ?? string.Empty;
            var lastName = user.LastName ?? string.Empty;
            var isActive = user.Enabled ?? false;

            if (!HasUserChanged(existingUser, email, firstName, lastName, isActive))
            {
                logger.LogInformation("Ignored unchanged Keycloak user {KeycloakUserId}", user.Id);
                return;
            }

            existingUser.Email = email;
            existingUser.FirstName = firstName;
            existingUser.LastName = lastName;
            existingUser.IsActive = isActive;
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
                EventType = EventType.UserUpdated
            });
            logger.LogInformation("Updated user for Keycloak ID {KeycloakUserId}", user.Id);
        }
    }

    private static bool HasUserChanged(UserEntity existingUser, string email, string firstName, string lastName,
        bool isActive)
    {
        return !string.Equals(existingUser.Email, email, StringComparison.Ordinal) ||
               !string.Equals(existingUser.FirstName, firstName, StringComparison.Ordinal) ||
               !string.Equals(existingUser.LastName, lastName, StringComparison.Ordinal) ||
               existingUser.IsActive != isActive;
    }


    /// <summary>
    /// Synchronizes all users from Keycloak to the local database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SyncAllUsersAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting full user synchronization");
        var startTime = DateTimeOffset.UtcNow;
        var totalUsers = await keycloakService.GetUsersCountAsync();
        var batches = (int)Math.Ceiling((double)totalUsers / userSyncOptions.BatchSize);
        var syncedKeycloakIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        logger.LogInformation("Synchronizing {TotalUsers} users in {Batches} batches", totalUsers, batches);

        for (int i = 0; i < batches; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var offset = i * userSyncOptions.BatchSize;
            var syncedCount = await SyncUsersBatchAsync(offset, userSyncOptions.BatchSize, syncedKeycloakIds);
            logger.LogInformation("Batch {BatchNumber}: Synced {SyncedCount} users", i + 1, syncedCount);

            // Small delay to avoid overwhelming Keycloak
            await Task.Delay(1000, cancellationToken);
        }

        var deactivatedCount = await DeactivateUsersMissingFromKeycloakAsync(syncedKeycloakIds, cancellationToken);

        var endTime = DateTimeOffset.UtcNow;
        logger.LogInformation(
            "Full user synchronization completed. Deactivated {DeactivatedCount} users missing from Keycloak",
            deactivatedCount);
        await notificationPublisher.Notify(new SyncUsersCompleteNotify(totalUsers, startTime, endTime));
    }

    /// <summary>
    /// Synchronizes a batch of users from Keycloak to the local database.
    /// </summary>
    /// <param name="offset">The offset for retrieving users from Keycloak.</param>
    /// <param name="limit">The maximum number of users to retrieve in this batch.</param>
    /// <param name="syncedKeycloakIds">Synced keycloak ids</param>
    /// <returns>The number of users successfully synchronized in this batch.</returns>
    private async Task<int> SyncUsersBatchAsync(int offset, int limit, ISet<string> syncedKeycloakIds)
    {
        var keycloakUsers = await keycloakService.GetUsersAsync(offset, limit);
        var syncedCount = 0;

        foreach (var keycloakUser in keycloakUsers)
        {
            if (keycloakUser.Id == null) continue;
            syncedKeycloakIds.Add(keycloakUser.Id);

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

    private async Task<int> DeactivateUsersMissingFromKeycloakAsync(ISet<string> syncedKeycloakIds,
        CancellationToken cancellationToken)
    {
        var localActiveUsers = await userRepository.GetActiveKeycloakUsersAsync(cancellationToken);
        var deactivatedCount = 0;

        foreach (var localUser in localActiveUsers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (syncedKeycloakIds.Contains(localUser.KeycloakId))
            {
                continue;
            }

            await DeactivateUserAsync(localUser);
            deactivatedCount++;

            logger.LogInformation(
                "Deactivated local user {UserId} because Keycloak ID {KeycloakUserId} was not found during full sync",
                localUser.UserId,
                localUser.KeycloakId);
        }

        return deactivatedCount;
    }

    private async Task DeactivateUserAsync(UserEntity user)
    {
        user.IsActive = false;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.LastSyncedAt = DateTimeOffset.UtcNow;
        await userRepository.UpdateAsync(user);
        await userService.PublishUserEvent(new UserChangeEvent()
        {
            User = new User()
            {
                KeycloakId = user.KeycloakId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserId = user.UserId,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt.ToTimestamp(),
                UpdatedAt = user.UpdatedAt.ToTimestamp(),
                LastSyncedAt = user.LastSyncedAt.ToTimestamp(),
            },
            EventTime = DateTimeOffset.Now.ToTimestamp(),
            EventType = EventType.UserDeactivated
        });
    }
}