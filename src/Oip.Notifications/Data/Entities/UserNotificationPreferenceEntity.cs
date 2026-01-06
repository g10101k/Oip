namespace Oip.Notifications.Data.Entities;

/// <summary>
/// User notification preferences
/// </summary>
public class UserNotificationPreferenceEntity
{
    /// <summary>
    /// Unique identifier for the user notification preference
    /// </summary>
    public int UserNotificationPreferenceId { get; set; }

    /// <summary>
    /// User identifier
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Notification type identifier
    /// </summary>
    public int NotificationTypeId { get; set; }

    /// <summary>
    /// Notification channel identifier
    /// </summary>
    public int NotificationChannelId { get; set; }

    /// <summary>
    /// Whether notifications are enabled for this preference
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Associated notification type
    /// </summary>
    public NotificationTypeEntity NotificationType { get; set; } = null!;

    /// <summary>
    /// Associated notification channel
    /// </summary>
    public NotificationChannelEntity NotificationChannel { get; set; } = null!;
}