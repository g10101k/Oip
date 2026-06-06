using Microsoft.EntityFrameworkCore;
using Oip.Data.Repositories;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.Repositories;

/// <summary>
/// Repository for managing notification types
/// </summary>
/// <param name="context">The database context</param>
public class NotificationTypeRepository(NotificationsDbContext context)
    : BaseRepository<NotificationTypeEntity, int>(context)
{
    /// <summary>
    /// Retrieves a notification type by its name
    /// </summary>
    /// <param name="name">The name of the notification type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The notification type if found; otherwise, null</returns>
    public async Task<NotificationTypeEntity?> GetByNameAsync(string name,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Name == name, cancellationToken);
    }

    /// <summary>
    /// Retrieves notification types by scope
    /// </summary>
    /// <param name="scope">The scope to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of notification types with the specified scope</returns>
    public async Task<List<NotificationTypeEntity>> GetByScopeAsync(string scope,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => e.Scope == scope).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves notification types with their associated templates
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of notification types with templates</returns>
    public async Task<List<NotificationTypeEntity>> GetWithTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Templates)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Inserts a new notification type or updates an existing one based on the name
    /// </summary>
    /// <param name="entity">The notification type entity to upsert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public async Task<NotificationTypeEntity> Upsert(NotificationTypeEntity entity,
        CancellationToken cancellationToken = default)
    {
        var notificationType = await DbSet.FirstOrDefaultAsync(e => e.Name == entity.Name, cancellationToken);

        if (notificationType != null)
        {
            // Обновляем существующую запись
            notificationType.Description = entity.Description;
            notificationType.Scope = entity.Scope;
            await UpdateAsync(notificationType, cancellationToken);
            return notificationType;
        }
        else
        {
            // Добавляем новую запись
            return await AddAsync(entity, cancellationToken);
        }
    }
}