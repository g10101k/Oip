using Microsoft.EntityFrameworkCore;
using Oip.Data.Repositories;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.Repositories;

/// <summary>
/// Repository for managing notification deliveries
/// </summary>
/// <param name="context">The database context</param>
public class NotificationDeliveryRepository(NotificationsDbContext context)
    : BaseRepository<NotificationDeliveryEntity, long>(context)
{
    /// <summary>
    /// Retrieves deliveries by status
    /// </summary>
    /// <param name="status">The delivery status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of deliveries with the specified status</returns>
    public async Task<List<NotificationDeliveryEntity>> GetByStatusAsync(Enums.DeliveryStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Include(d => d.NotificationChannel)
            .Where(d => d.Status == status)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves deliveries for a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of deliveries for the specified user</returns>
    public async Task<List<NotificationDeliveryEntity>> GetByUserIdAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Include(d => d.NotificationChannel)
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves deliveries for a specific channel
    /// </summary>
    /// <param name="channelId">The channel identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of deliveries for the specified channel</returns>
    public async Task<List<NotificationDeliveryEntity>> GetByChannelIdAsync(int channelId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Where(d => d.NotificationChannelId == channelId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves failed deliveries eligible for retry
    /// </summary>
    /// <param name="maxRetryCount">The maximum retry count</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of deliveries eligible for retry</returns>
    public async Task<List<NotificationDeliveryEntity>> GetForRetryAsync(int maxRetryCount,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Include(d => d.NotificationChannel)
            .Where(d => d.Status == Enums.DeliveryStatus.Failed &&
                        d.RetryCount < maxRetryCount &&
                        d.NotificationChannel.IsActive)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a delivery by its external identifier
    /// </summary>
    /// <param name="externalId">The external identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The delivery if found; otherwise, null</returns>
    public async Task<NotificationDeliveryEntity?> GetByExternalIdAsync(string externalId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Include(d => d.NotificationChannel)
            .FirstOrDefaultAsync(d => d.ExternalId == externalId, cancellationToken);
    }

    /// <summary>
    /// Updates the status of a delivery
    /// </summary>
    /// <param name="deliveryId">The delivery identifier</param>
    /// <param name="status">The new status</param>
    /// <param name="externalId">The external identifier (optional)</param>
    /// <param name="errorMessage">The error message if applicable (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="ArgumentException">Thrown when delivery is not found</exception>
    public async Task UpdateStatusAsync(long deliveryId, Enums.DeliveryStatus status, string? externalId = null,
        string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        var delivery = await GetByIdAsync(deliveryId, cancellationToken);
        if (delivery == null)
            throw new ArgumentException($"Delivery with id {deliveryId} not found");
        delivery.Status = status;
        delivery.ExternalId = externalId ?? delivery.ExternalId;
        delivery.ErrorMessage = errorMessage ?? delivery.ErrorMessage;
        if (status == Enums.DeliveryStatus.Sent)
            delivery.SentAt = DateTimeOffset.UtcNow;
        else if (status == Enums.DeliveryStatus.Delivered)
            delivery.DeliveredAt = DateTimeOffset.UtcNow;
        if (status == Enums.DeliveryStatus.Failed)
            delivery.RetryCount++;
        await UpdateAsync(delivery, cancellationToken);
    }
}