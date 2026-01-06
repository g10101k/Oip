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
    /// Gets or sets the maximum number of retry attempts for sending notifications through this channel
    /// </summary>
    int MaxRetryCount { get; set; }

    /// <summary>
    /// Channel activity
    /// </summary>
    bool IsEnable { get; }

    /// <summary>
    /// Whether the channel requires user verification
    /// </summary>
    bool RequiresVerification { get; set; }

    /// <summary>
    /// Opening the channel
    /// </summary>
    void OpenChannel();

    /// <summary>
    /// Closing the channel
    /// </summary>
    void CloseChannel();

    /// <summary>
    /// Notification with attachment
    /// </summary>
    void ProcessNotify(NotificationDto message, CancellationToken cancellationToken = default);
}

public class UserInfoDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}

public class NotificationDto
{
    public UserInfoDto UserInfo { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public Attachment[] Attachment { get; set; }
}