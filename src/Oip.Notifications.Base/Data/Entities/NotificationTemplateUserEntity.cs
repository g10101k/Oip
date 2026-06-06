namespace Oip.Notifications.Data.Entities;

/// <summary>
/// Mapping between a notification template and a user
/// </summary>
public class NotificationTemplateUserEntity
{
    /// <summary>
    /// Unique identifier for the template-user mapping
    /// </summary>
    public int NotificationTemplateUserId { get; set; }

    /// <summary>
    /// Notification template identifier
    /// </summary>
    public int NotificationTemplateId { get; set; }

    /// <summary>
    /// User identifier
    /// </summary>
    public int UserId { get; set; }
}