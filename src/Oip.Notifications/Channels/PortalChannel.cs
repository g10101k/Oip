using Microsoft.AspNetCore.SignalR;
using Oip.Notifications.Base;
using Oip.Notifications.Hubs;

namespace Oip.Notifications.Channels;

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

public record PortalMessage(string Subject, string Message, string Severity);