using Microsoft.EntityFrameworkCore;
using Oip.Data.Repositories;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.Repositories;

/// <summary>
/// Repository for managing notification templates
/// </summary>
/// <param name="context">The database context</param>
public class NotificationTemplateRepository(NotificationsDbContext context)
    : BaseRepository<NotificationTemplateEntity, int>(context)
{
    /// <summary>
    /// Retrieves active templates for a specific notification type
    /// </summary>
    /// <param name="notificationTypeId">The notification type identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of active templates for the specified type</returns>
    public async Task<List<NotificationTemplateEntity>> GetActiveTemplatesByTypeAsync(
        int notificationTypeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.NotificationTemplateChannels)
            .ThenInclude(tc => tc.NotificationChannel)
            .Include(e => e.NotificationTemplateUsers)
            .Where(e => e.NotificationTypeId == notificationTypeId && e.IsActive)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a template with its associated channels
    /// </summary>
    /// <param name="id">The template identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The template with channels if found; otherwise, null</returns>
    public async Task<NotificationTemplateEntity?> GetWithChannelsAsync(int id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.NotificationTemplateChannels)
            .ThenInclude(tc => tc.NotificationChannel)
            .AsSplitQuery()
            .Include(e => e.NotificationType)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.NotificationTemplateId == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves templates for a specific notification type with their associated channels
    /// </summary>
    /// <param name="notificationTypeId">The notification type identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of templates with channels for the specified type</returns>
    public async Task<List<NotificationTemplateEntity>> GetByTypeWithChannelsAsync(
        int notificationTypeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.NotificationTemplateChannels)
            .ThenInclude(tc => tc.NotificationChannel)
            .Where(e => e.NotificationTypeId == notificationTypeId && e.IsActive)
            .ToListAsync(cancellationToken);
    }
}