using Microsoft.AspNetCore.SignalR;
using Oip.Notifications.Base;
using Oip.Notifications.Hubs;

namespace Oip.Notifications.Channels;

/// <summary>
/// Portal notification channel that sends messages through SignalR hub to user groups
/// Uses the NotificationHub to deliver portal messages with specified subject, content and severity
/// </summary>
/// <param name="notificationHub">SignalR hub context for sending real-time notifications to connected clients</param>
public class PortalChannel(IHubContext<NotificationHub> notificationHub) : INotificationChannel
{
    /// <inheritdoc />
    public Queue<NotificationDto> Queue { get; set; } = null!;

    /// <inheritdoc />

    public string Code { get; set; } = typeof(PortalChannel).FullName!;

    /// <inheritdoc />
    public string Name { get; set; } = "Portal";

    /// <inheritdoc />
    public int MaxRetryCount { get; set; } = 1;

    /// <inheritdoc />
    public bool IsEnable { get; set; }

    /// <inheritdoc />
    public bool RequiresVerification { get; set; }

    /// <inheritdoc />
    public void ProcessNotify(NotificationDto message, CancellationToken cancellationToken = default)
    {
        var msg = new PortalMessage(message.Subject, message.Message, "info");
        notificationHub.Clients.Group($"{message.User.UserId}")
            .SendAsync("ReceiveNotification", msg, cancellationToken: cancellationToken).Wait(cancellationToken);
    }
}

/// <summary>
/// Portal notification message containing subject, content and severity level
/// </summary>
/// <param name="Subject">Notification subject/title displayed to the user</param>
/// <param name="Message">Notification content/message body displayed to the user</param>
/// <param name="Severity">Message severity level indicating importance</param>
public record PortalMessage(string Subject, string Message, string Severity);