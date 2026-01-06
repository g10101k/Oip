namespace Oip.Notifications.Data.Entities;

/// <summary>
/// Notification/event
/// </summary>
public class NotificationEntity
{
    /// <summary>
    /// Unique identifier for the notification
    /// </summary>
    public long NotificationId { get; set; }

    /// <summary>
    /// Notification type identifier
    /// </summary>
    public int NotificationTypeId { get; set; }

    /// <summary>
    /// Creation timestamp of the notification
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// JSON data associated with the notification
    /// </summary>
    public string? DataJson { get; set; }

    /// <summary>
    /// Associated notification type
    /// </summary>
    public NotificationTypeEntity NotificationType { get; set; } = null!;

    /// <summary>
    /// Users associated with this notification
    /// </summary>
    public List<NotificationUserEntity> NotificationUsers { get; set; } = new();
}