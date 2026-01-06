namespace Oip.Notifications.Data.Entities;

/// <summary>
/// Users who should be notified about an event
/// </summary>
public class NotificationUserEntity
{
    /// <summary>
    /// Unique identifier for the notification user
    /// </summary>
    public long NotificationUserId { get; set; }

    /// <summary>
    /// Notification identifier
    /// </summary>
    public long NotificationId { get; set; }

    /// <summary>
    /// User identifier
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Subject of the notification for this user
    /// </summary>
    public string Subject { get; set; } = null!;

    /// <summary>
    /// Message of the notification for this user
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Importance level of the notification
    /// </summary>
    public ImportanceLevel Importance { get; set; }

    /// <summary>
    /// Associated notification
    /// </summary>
    public NotificationEntity Notification { get; set; } = null!;

    /// <summary>
    /// Delivery attempts for this notification user
    /// </summary>
    public List<NotificationDeliveryEntity> Deliveries { get; set; } = new();
}