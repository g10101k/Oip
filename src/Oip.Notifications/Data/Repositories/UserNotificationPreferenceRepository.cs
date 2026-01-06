using Microsoft.EntityFrameworkCore;
using Oip.Data.Repositories;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.Repositories;

/// <summary>
/// Repository for managing user notification preferences
/// </summary>
/// <param name="context">The database context</param>
public class UserNotificationPreferenceRepository(NotificationsDbContext context)
    : BaseRepository<UserNotificationPreferenceEntity, int>(context)
{
    /// <summary>
    /// Retrieves notification preferences for a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of preferences for the specified user</returns>
    public async Task<List<UserNotificationPreferenceEntity>> GetByUserIdAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.NotificationType)
            .Include(p => p.NotificationChannel)
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves notification preferences for a specific user and notification type
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="notificationTypeId">The notification type identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of preferences for the specified user and type</returns>
    public async Task<List<UserNotificationPreferenceEntity>> GetByUserAndTypeAsync(
        int userId, int notificationTypeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.NotificationChannel)
            .Where(p => p.UserId == userId && p.NotificationTypeId == notificationTypeId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a specific preference for a user, notification type, and channel
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="notificationTypeId">The notification type identifier</param>
    /// <param name="notificationChannelId">The notification channel identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The preference if found; otherwise, null</returns>
    public async Task<UserNotificationPreferenceEntity?> GetPreferenceAsync(
        int userId, int notificationTypeId, int notificationChannelId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.NotificationType)
            .Include(p => p.NotificationChannel)
            .FirstOrDefaultAsync(p => p.UserId == userId &&
                                      p.NotificationTypeId == notificationTypeId &&
                                      p.NotificationChannelId == notificationChannelId,
                cancellationToken);
    }

    /// <summary>
    /// Inserts or updates a notification preference
    /// </summary>
    /// <param name="preference">The preference to upsert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated or inserted preference</returns>
    public async Task<UserNotificationPreferenceEntity> UpsertPreferenceAsync(
        UserNotificationPreferenceEntity preference, CancellationToken cancellationToken = default)
    {
        var existing = await GetPreferenceAsync(
            preference.UserId,
            preference.NotificationTypeId,
            preference.NotificationChannelId,
            cancellationToken);
        if (existing != null)
        {
            existing.IsEnabled = preference.IsEnabled;
            await UpdateAsync(existing, cancellationToken);
            return existing;
        }
        else
        {
            await AddAsync(preference, cancellationToken);
            return preference;
        }
    }

    /// <summary>
    /// Deletes all notification preferences for a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DeleteByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var preferences = await GetByUserIdAsync(userId, cancellationToken);
        DbSet.RemoveRange(preferences);
        await context.SaveChangesAsync(cancellationToken);
    }
}