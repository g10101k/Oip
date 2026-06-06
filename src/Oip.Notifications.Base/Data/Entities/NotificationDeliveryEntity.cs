namespace Oip.Notifications.Data.Entities;

/// <summary>
/// Notification delivery history by channels
/// </summary>
public class NotificationDeliveryEntity
{
    /// <summary>
    /// Unique identifier for the notification delivery
    /// </summary>
    public long NotificationDeliveryId { get; set; }

    /// <summary>
    /// Notification user identifier
    /// </summary>
    public long NotificationUserId { get; set; }

    /// <summary>
    /// User identifier
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Notification channel identifier
    /// </summary>
    public int NotificationChannelId { get; set; }

    /// <summary>
    /// Delivery status
    /// </summary>
    public Enums.DeliveryStatus Status { get; set; }

    /// <summary>
    /// External identifier from the delivery service
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Error message if delivery failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of delivery retry attempts
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Creation timestamp of the delivery record
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the notification was sent
    /// </summary>
    public DateTimeOffset? SentAt { get; set; }

    /// <summary>
    /// Timestamp when the notification was delivered
    /// </summary>
    public DateTimeOffset? DeliveredAt { get; set; }

    /// <summary>
    /// Associated notification user
    /// </summary>
    public NotificationUserEntity NotificationUser { get; set; } = null!;

    /// <summary>
    /// Associated notification channel
    /// </summary>
    public NotificationChannelEntity NotificationChannel { get; set; } = null!;
}