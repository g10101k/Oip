namespace Oip.Notifications.Data.Entities;

/// <summary>
/// Association between a notification template and a notification channel
/// </summary>
public class NotificationTemplateChannelEntity
{
    /// <summary>
    /// Unique identifier for the template-channel association
    /// </summary>
    public int NotificationTemplateChannelId { get; set; }

    /// <summary>
    /// Notification template identifier
    /// </summary>
    public int NotificationTemplateId { get; set; }

    /// <summary>
    /// Notification channel identifier
    /// </summary>
    public int NotificationChannelId { get; set; }

    /// <summary>
    /// Associated notification template
    /// </summary>
    public NotificationTemplateEntity NotificationTemplate { get; set; } = null!;

    /// <summary>
    /// Associated notification channel
    /// </summary>
    public NotificationChannelEntity NotificationChannel { get; set; } = null!;
}