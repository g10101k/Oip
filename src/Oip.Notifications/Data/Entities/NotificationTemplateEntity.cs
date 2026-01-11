using Oip.Notifications.Base;

namespace Oip.Notifications.Data.Entities;

/// <summary>
/// Notification template for different channels
/// </summary>
public class NotificationTemplateEntity
{
    /// <summary>
    /// Unique identifier for the notification template
    /// </summary>
    public int NotificationTemplateId { get; set; }

    /// <summary>
    /// Unique identifier for the notification type
    /// </summary>
    public int NotificationTypeId { get; set; }

    /// <summary>
    /// Subject template for the notification
    /// </summary>
    public string SubjectTemplate { get; set; } = null!;

    /// <summary>
    /// Message template for the notification
    /// </summary>
    public string MessageTemplate { get; set; } = null!;

    /// <summary>
    /// Importance level of the notification
    /// </summary>
    public ImportanceLevel Importance { get; set; }

    /// <summary>
    /// Whether the notification template is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Associated notification type
    /// </summary>
    public NotificationTypeEntity NotificationType { get; set; } = null!;

    /// <summary>
    /// Channels associated with this template
    /// </summary>
    public List<NotificationTemplateChannelEntity> NotificationTemplateChannels { get; set; } = new();

    /// <summary>
    /// Users associated with this template
    /// </summary>
    public List<NotificationTemplateUserEntity> NotificationTemplateUsers { get; set; } = new();
}