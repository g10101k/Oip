namespace Oip.Notifications.Base.Services;

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