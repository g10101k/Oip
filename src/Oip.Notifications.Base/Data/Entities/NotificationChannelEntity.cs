namespace Oip.Notifications.Data.Entities;

/// <summary>
/// Notification delivery channels
/// </summary>
public class NotificationChannelEntity
{
    /// <summary>
    /// Unique identifier for the notification channel
    /// </summary>
    public int NotificationChannelId { get; set; }

    /// <summary>
    /// Name of the notification channel
    /// </summary>
    public required string Code { get; set; } = null!;
    
    /// <summary>
    /// Name of the notification channel
    /// </summary>
    public required string Name { get; set; } = null!;

    /// <summary>
    /// Whether the channel is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the channel requires user verification
    /// </summary>
    public bool RequiresVerification { get; set; }

    /// <summary>
    /// Maximum number of delivery retry attempts
    /// </summary>
    public int? MaxRetryCount { get; set; }

    /// <summary>
    /// Collection of notification templates associated with this channel
    /// </summary>
    public List<NotificationTemplateEntity> Templates { get; set; } = new();

    /// <summary>
    /// User-specific preferences for this channel
    /// </summary>
    public List<UserNotificationPreferenceEntity> UserPreferences { get; set; } = new();

    /// <summary>
    /// List of deliveries associated with this channel
    /// </summary>
    public List<NotificationDeliveryEntity> Deliveries { get; set; } = new();
}