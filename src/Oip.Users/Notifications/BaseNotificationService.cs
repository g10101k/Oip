using Newtonsoft.Json;
using Oip.Base.Runtime;
using Oip.Notifications;

namespace Oip.Users.Notifications;

/// <summary>
/// BaseNotificationService is an abstract base class that implements IStatic method ExecuteAsync registers notification
/// types with a gRPC service during application startupStartupTask to register notification types with a gRPC
/// notification service during application startup
/// </summary>
public class BaseNotificationService(
    GrpcNotificationService.GrpcNotificationServiceClient client,
    ILogger<BaseNotificationService> logger)
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

    public async Task Notify<TNotification>(TNotification notification, ImportanceLevel level)
    {
        var q = NotificationTypes.FirstOrDefault(x => x.Name == typeof(TNotification).FullName);
        await client.CreateNotificationAsync(new CreateNotificationRequest()
        {
            NotificationTypeId = q.NotificationTypeId,
            DataJson = JsonConvert.SerializeObject(notification)
        });
    }
}

public class NotificationStartup(
    GrpcNotificationService.GrpcNotificationServiceClient client,
    ILogger<BaseNotificationService> logger,
    BaseNotificationService notificationService
) : IStartupTask
{
    /// <inheritdoc />
    public int Order { get; } = 0;

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
            var response = await client.CreateNotificationTypesAsync(request);
            notificationService.NotificationTypes = response.NotificationType.ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }
}