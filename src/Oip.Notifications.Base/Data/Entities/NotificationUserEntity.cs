using Oip.Notifications.Base;

namespace Oip.Notifications.Data.Entities;

public class NotificationUserEntity
{
    public long NotificationUserId { get; set; }

    public long NotificationId { get; set; }

    public int UserId { get; set; }

    public string Subject { get; set; } = null!;

    public string Message { get; set; } = null!;

    public ImportanceLevel Importance { get; set; }

    public int? NotificationChannelId { get; set; }

    public DateTimeOffset? SentAt { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }

    public DateTimeOffset? ReadAt { get; set; }

    public NotificationEntity Notification { get; set; } = null!;

    public List<NotificationDeliveryEntity> Deliveries { get; set; } = new();
}