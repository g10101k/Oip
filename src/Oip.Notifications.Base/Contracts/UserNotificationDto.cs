namespace Oip.Notifications.Base.Contracts;

/// <summary>
/// User notification for displaying in the portal.
/// </summary>
public record UserNotificationDto(
    long NotificationUserId,
    long NotificationId,
    int NotificationTypeId,
    string NotificationTypeName,
    string Subject,
    string Message,
    ImportanceLevel Importance,
    int? NotificationChannelId,
    DateTimeOffset? SentAt,
    DateTimeOffset? DeliveredAt,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt,
    string? DataJson);

/// <summary>
/// Paged response with current user notifications.
/// </summary>
public record UserNotificationListResponse(
    IReadOnlyList<UserNotificationDto> Notifications,
    int TotalCount);

/// <summary>
/// Response with current user notification count.
/// </summary>
public record UserNotificationCountResponse(int Count);

/// <summary>
/// Request to send a test notification to the current user.
/// </summary>
/// <param name="Subject">Notification subject.</param>
/// <param name="Message">Notification text.</param>
public record CreateTestNotificationRequest(string Subject, string Message);
