using System.Net.Mail;

namespace Oip.Notifications.Base;

/// <summary>
/// Defines a channel for sending notifications
/// </summary>
public interface INotificationChannel
{
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
    /// Notification
    /// </summary>
    void Notify(UserInfoDto userInfoDto, string subject, string message);

    /// <summary>
    /// Notification with attachment
    /// </summary>
    /// <param name="userInfoDto"></param>
    /// <param name="subject"></param>
    /// <param name="message"></param>
    /// <param name="attachments"></param>
    void Notify(UserInfoDto userInfoDto, string subject, string message, Attachment[] attachments);
}

public class UserInfoDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}