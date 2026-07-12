using Newtonsoft.Json;
using Oip.Notifications.Base;
using Oip.Notifications.Base.Services;

namespace Oip.Users.Base.Notifications;


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
