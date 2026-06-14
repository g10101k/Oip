namespace Oip.Notifications.Base;

/// <summary>
/// Provides notification service operations without binding callers to a transport.
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
