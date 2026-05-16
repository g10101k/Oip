using Newtonsoft.Json;
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
/// Provides the notification service operations required by users without binding callers to a transport.
/// </summary>
public interface INotificationServiceClient
{
    /// <summary>
    /// Creates notification types in the configured notification service.
    /// </summary>
    Task<CreateNotificationTypesResponse> CreateNotificationTypesAsync(
        CreateNotificationTypesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a notification in the configured notification service.
    /// </summary>
    Task CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Calls the remote notifications gRPC service.
/// </summary>
public class GrpcNotificationServiceClientAdapter(GrpcNotificationService.GrpcNotificationServiceClient client)
    : INotificationServiceClient
{
    /// <inheritdoc />
    public async Task<CreateNotificationTypesResponse> CreateNotificationTypesAsync(
        CreateNotificationTypesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.CreateNotificationTypesAsync(request, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateNotificationAsync(CreateNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        await client.CreateNotificationAsync(request, cancellationToken: cancellationToken);
    }
}

public class BaseNotificationService(INotificationServiceClient client)
    : INotificationPublisher
{
    internal List<NotificationType> NotificationTypes { get; set; } =
    [
        new()
        {
            Name = typeof(SyncUsersCompleteNotify).FullName,
            Description = "Sync users complete notifications.",
            Scope = typeof(SyncUsersCompleteNotify).Assembly.GetName().Name
        },
        new()
        {
            Name = typeof(CustomUserNotify).FullName,
            Description = "Custom user notifications.",
            Scope = typeof(CustomUserNotify).Assembly.GetName().Name
        }
    ];

    public async Task Notify<TNotification>(TNotification notification)
    {
        await client.CreateNotificationAsync(new CreateNotificationRequest()
        {
            NotificationType = typeof(TNotification).FullName,
            DataJson = JsonConvert.SerializeObject(notification)
        });
    }
}