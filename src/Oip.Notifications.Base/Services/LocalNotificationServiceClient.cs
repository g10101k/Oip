using Oip.Notifications.Base;

namespace Oip.Notifications.Services;

/// <summary>
/// Calls the in-process notifications service when modules are composed in standalone mode.
/// </summary>
public class LocalNotificationServiceClient(NotificationService notificationService) : INotificationServiceClient
{
    /// <inheritdoc />
    public async Task<CreateNotificationTypesResponse> CreateNotificationTypesAsync(
        CreateNotificationTypesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await notificationService.CreateNotificationTypes(request, null!);
    }

    /// <inheritdoc />
    public async Task CreateNotificationAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        await notificationService.CreateNotification(request, null!);
    }
}
