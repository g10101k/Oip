using System.Net.Mail;

namespace Oip.Notifications.Base;

/// <summary>
/// Defines a channel for sending notifications
/// </summary>
public interface INotificationChannel
{
    /// <summary>
    /// Gets or sets the queue of notifications to be processed by the channel
    /// </summary>
    Queue<NotificationDto> Queue { get; set; }

    /// <summary>
    /// Unique identifier for the notification channel
    /// </summary>
    string Code { get; set; }

    /// <summary>
    /// Channel name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Maximum number of retry attempts for sending notifications through this channel
    /// </summary>
    int MaxRetryCount { get; set; }

    /// <summary>
    /// Channel activity
    /// </summary>
    bool IsEnable { get; set; }

    /// <summary>
    /// Whether the channel requires user verification
    /// </summary>
    bool RequiresVerification { get; set; }

    /// <summary>
    /// Notification with attachment
    /// </summary>
    void ProcessNotify(NotificationDto message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contains user identification and contact information
/// </summary>
public class UserInfoDto
{
    /// <summary>
    /// Unique identifier for the user in the system
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Email address associated with the user
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Phone number associated with the user for contact and notification purposes
    /// </summary>
    public string Phone { get; set; } = null!;
}

/// <summary>
/// Notification data container holding message content, recipient information, and file attachments
/// </summary>
public class NotificationDto
{
    /// <summary>
    /// User entity containing account identification and contact details
    /// </summary>
    public UserInfoDto User { get; set; } = null!;

    /// <summary>
    /// Subject line of the notification message
    /// </summary>
    public string Subject { get; set; } = null!;

    /// <summary>
    /// Notification message content to be sent to the recipient
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Importance level of the notification indicating priority for delivery handling
    /// </summary>
    public ImportanceLevel ImportanceLevel { get; set; } 

    /// <summary>
    /// File attachments included with the notification
    /// </summary>
    public Attachment[] Attachment { get; set; } = null!;
}