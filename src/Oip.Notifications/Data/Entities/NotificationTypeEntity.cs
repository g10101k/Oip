namespace Oip.Notifications.Data.Entities;

/// <summary>
/// Type of notification within the system
/// </summary>
public class NotificationTypeEntity
{
    /// <summary>
    /// Unique identifier for the notification type
    /// </summary>
    public int NotificationTypeId { get; set; }

    /// <summary>
    /// Name of the notification type
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Detailed explanation of the notification type
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Scope of notification type (global, appName, feature)
    /// </summary>
    public required string Scope { get; set; }

    /// <summary>
    /// Collection of notification templates associated with this notification type
    /// </summary>
    public List<NotificationTemplateEntity> Templates { get; set; } = new();

    /// <summary>
    /// User preferences for notifications of this type
    /// </summary>
    public List<UserNotificationPreferenceEntity> UserPreferences { get; set; } = new();
}