using Microsoft.EntityFrameworkCore;
using Oip.Data.Repositories;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.Repositories;

/// <summary>
/// Repository for managing notifications
/// </summary>
/// <param name="context">The database context</param>
public class NotificationRepository(NotificationsDbContext context)
    : BaseRepository<NotificationEntity, long>(context)
{
    /// <summary>
    /// Retrieves notifications by type
    /// </summary>
    /// <param name="notificationTypeId">The notification type identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of notifications for the specified type</returns>
    public async Task<List<NotificationEntity>> GetByTypeAsync(int notificationTypeId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.NotificationTypeId == notificationTypeId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves notifications created after a specific date
    /// </summary>
    /// <param name="date">The cutoff date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of notifications created after the specified date</returns>
    public async Task<List<NotificationEntity>> GetCreatedAfterAsync(DateTimeOffset date,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.CreatedAt >= date)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a notification with its associated users
    /// </summary>
    /// <param name="id">The notification identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The notification with users if found; otherwise, null</returns>
    public async Task<NotificationEntity?> GetWithUsersAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(n => n.NotificationUsers)
            .Include(n => n.NotificationType)
            .FirstOrDefaultAsync(n => n.NotificationId == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves notifications for a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of notifications for the specified user</returns>
    public async Task<List<NotificationEntity>> GetByUserIdAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(n => n.NotificationType)
            .Include(n => n.NotificationUsers)
            .Where(n => n.NotificationUsers.Any(u => u.UserId == userId))
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}