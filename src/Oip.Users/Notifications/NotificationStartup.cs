using Oip.Base.Runtime;
using Oip.Notifications.Base;

namespace Oip.Users.Notifications;

/// <summary>
/// NotificationStartup is an IStartupTask that registers notification types with a gRPC notification service during application startup
/// </summary>
public class NotificationStartup(
    INotificationServiceClient client,
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