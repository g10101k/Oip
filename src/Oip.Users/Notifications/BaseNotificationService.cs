using Newtonsoft.Json;
using Oip.Base.Runtime;
using Oip.Notifications.Base;

namespace Oip.Users.Notifications;

/// <summary>
/// BaseNotificationService is an abstract base class that implements IStatic method ExecuteAsync registers notification
/// types with a gRPC service during application startupStartupTask to register notification types with a gRPC
/// notification service during application startup
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// Sends a notification payload to the configured notification transport.
    /// </summary>
    Task Notify<TNotification>(TNotification notification);
}

/// <summary>
/// Publishes notifications through the notifications gRPC service.
/// </summary>
public class BaseNotificationService(
    GrpcNotificationService.GrpcNotificationServiceClient client)
    : INotificationPublisher
{
    /// <summary>
    /// Gets or sets the list of notification types that are registered with the gRPC notification service during
    /// application startup.
    /// </summary>
    internal List<NotificationType> NotificationTypes { get; set; } =
    [
        new()
        {
            Name = typeof(SyncUsersCompleteNotify).FullName,
            Description = "Sync users complete notifications.",
            Scope = typeof(SyncUsersCompleteNotify).Assembly.GetName().Name
        }
    ];

    /// <summary>
    /// Sends a notification with the specified importance level.
    /// </summary>
    /// <param name="notification">The notification object to send.</param>
    /// <typeparam name="TNotification">The type of the notification object.</typeparam>
    public async Task Notify<TNotification>(TNotification notification)
    {
        var notificationType = NotificationTypes.FirstOrDefault(x => x.Name == typeof(TNotification).FullName) ??
                               throw new InvalidOperationException($"Notification type not found {typeof(TNotification).FullName}");

        await client.CreateNotificationAsync(new CreateNotificationRequest()
        {
            NotificationTypeId = notificationType.NotificationTypeId,
            DataJson = JsonConvert.SerializeObject(notification)
        });
    }
}

/// <summary>
/// Safe no-op publisher used when notifications are hosted in-process without a loopback client.
/// </summary>
public class NoOpNotificationPublisher(ILogger<NoOpNotificationPublisher> logger) : INotificationPublisher
{
    /// <inheritdoc />
    public Task Notify<TNotification>(TNotification notification)
    {
        logger.LogDebug("Skipping notification publishing for {NotificationType} in local module mode", typeof(TNotification).FullName);
        return Task.CompletedTask;
    }
}

/// <summary>
/// NotificationStartup is an IStartupTask that registers notification types with a gRPC notification service during application startup
/// </summary>
public class NotificationStartup(
    GrpcNotificationService.GrpcNotificationServiceClient client,
    ILogger<BaseNotificationService> logger,
    BaseNotificationService notificationService
) : IStartupTask
{
    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateNotificationTypesRequest();
            request.Requests.AddRange(notificationService.NotificationTypes.Select(x =>
                new CreateNotificationTypeRequest()
                {
                    Name = x.Name,
                    Description = x.Description,
                    Scope = x.Scope,
                }));
            var response = await client.CreateNotificationTypesAsync(request, cancellationToken: cancellationToken);
            notificationService.NotificationTypes = response.NotificationType.ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }
}
