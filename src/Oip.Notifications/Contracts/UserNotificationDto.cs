using Oip.Notifications.Base;

namespace Oip.Notifications.Contracts;

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
