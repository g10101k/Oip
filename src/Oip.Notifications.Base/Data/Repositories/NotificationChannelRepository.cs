using Microsoft.EntityFrameworkCore;
using Oip.Data.Repositories;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.Repositories;

/// <summary>
/// Repository for managing notification channels
/// </summary>
/// <param name="context">The database context</param>
public class NotificationChannelRepository(NotificationsDbContext context)
    : BaseRepository<NotificationChannelEntity, int>(context)
{
    /// <summary>
    /// Retrieves a notification channel by its name
    /// </summary>
    /// <param name="name">The name of the notification channel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The notification channel if found; otherwise, null</returns>
    public async Task<NotificationChannelEntity?> GetByNameAsync(string name,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Name == name, cancellationToken);
    }

    /// <summary>
    /// Retrieves all active notification channels
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of active notification channels</returns>
    public async Task<List<NotificationChannelEntity>> GetActiveChannelsAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => e.IsActive).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves notification channels that require verification
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of channels requiring verification</returns>
    public async Task<List<NotificationChannelEntity>> GetChannelsRequiringVerificationAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => e.RequiresVerification).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a new notification channel entity or skips creation if an entity with the same code already exists
    /// </summary>
    /// <param name="entity">The notification channel entity to create</param>
    /// <returns>The created or existing notification channel entity</returns>
    public async Task<NotificationChannelEntity> CreateOrSkip(NotificationChannelEntity entity)
    {
        var existingEntity = await DbSet.FirstOrDefaultAsync(e => e.Code == entity.Code);
        if (existingEntity == null)
        {
            await DbSet.AddAsync(entity);
        }
        await context.SaveChangesAsync();
        return entity;
    }
}