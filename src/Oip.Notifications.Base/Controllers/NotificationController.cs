using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Data.Constants;
using Oip.Base.Exceptions;
using Oip.Base.Services;
using Oip.Notifications.Base.Channels;
using Oip.Notifications.Base.Contracts;
using Oip.Notifications.Base.Data.Contexts;
using Oip.Notifications.Base.Data.Entities;
using Oip.Notifications.Base.Services;

namespace Oip.Notifications.Base.Controllers;

/// <summary>
/// Provides API endpoints for current user notifications.
/// </summary>
[ApiController]
[Authorize]
[Route("api/notification")]
[ApiExplorerSettings(GroupName = "notification")]
public class NotificationController(
    NotificationsDbContext context,
    IUserService userDirectory,
    ClaimService currentClaimService,
    ChannelService channelService,
    IUserCacheRepository userCache) : ControllerBase
{
    /// <summary>
    /// Notification type of the notifications sent by <see cref="CreateTestNotificationAsync"/>.
    /// </summary>
    public const string TestNotificationType = "Oip.Notifications.Base.TestNotification";

    /// <summary>
    /// Gets notifications for the current user.
    /// </summary>
    [HttpGet("get-notification-by-user")]
    [ProducesResponseType<UserNotificationListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserNotificationListResponse>> GetNotificationByUserAsync(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] bool unreadOnly = true,
        CancellationToken cancellationToken = default)
    {
        if (skip < 0)
            throw new ApiException("Invalid notification request", "Skip must be greater than or equal to 0.",
                StatusCodes.Status400BadRequest);

        if (take is < 1 or > 100)
            throw new ApiException("Invalid notification request", "Take must be between 1 and 100.",
                StatusCodes.Status400BadRequest);

        var userId = await GetCurrentUserIdAsync(cancellationToken);

        var query = context.NotificationUsers
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(x => x.ReadAt == null);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var notifications = await query
            .OrderByDescending(x => x.Notification.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(x => new UserNotificationDto(
                x.NotificationUserId,
                x.NotificationId,
                x.Notification.NotificationTypeId,
                x.Notification.NotificationType.Name,
                x.Subject,
                x.Message,
                x.Importance,
                x.NotificationChannelId,
                x.SentAt,
                x.DeliveredAt,
                x.ReadAt,
                x.Notification.CreatedAt,
                x.Notification.DataJson))
            .ToListAsync(cancellationToken);

        return Ok(new UserNotificationListResponse(notifications, totalCount));
    }

    /// <summary>
    /// Gets the current user notification count.
    /// </summary>
    [HttpGet("get-notification-count-by-user")]
    [ProducesResponseType<UserNotificationCountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserNotificationCountResponse>> GetNotificationCountByUserAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = await GetCurrentUserIdAsync(cancellationToken);
        var count = await context.NotificationUsers
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId && x.ReadAt == null, cancellationToken);

        return Ok(new UserNotificationCountResponse(count));
    }

    /// <summary>
    /// Marks a current user notification as read.
    /// </summary>
    [HttpPost("mark-notification-as-read/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> MarkNotificationAsReadAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken = default)
    {
        var userId = await GetCurrentUserIdAsync(cancellationToken);

        var notification = await context.NotificationUsers
            .FirstOrDefaultAsync(x => x.NotificationUserId == id && x.UserId == userId, cancellationToken);

        if (notification is null)
        {
            throw new ApiException("Notification not found", $"Notification with id {id} was not found.",
                StatusCodes.Status404NotFound);
        }

        notification.ReadAt ??= DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Gets a current user notification by identifier.
    /// </summary>
    [HttpGet("get-notification-by-id")]
    [ProducesResponseType<UserNotificationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserNotificationDto>> GetNotificationByIdAsync(
        [FromQuery] long id,
        CancellationToken cancellationToken = default)
    {
        var userId = await GetCurrentUserIdAsync(cancellationToken);

        var notification = await context.NotificationUsers
            .AsNoTracking()
            .Where(x => x.NotificationUserId == id && x.UserId == userId)
            .Select(x => new UserNotificationDto(
                x.NotificationUserId,
                x.NotificationId,
                x.Notification.NotificationTypeId,
                x.Notification.NotificationType.Name,
                x.Subject,
                x.Message,
                x.Importance,
                x.NotificationChannelId,
                x.SentAt,
                x.DeliveredAt,
                x.ReadAt,
                x.Notification.CreatedAt,
                x.Notification.DataJson))
            .FirstOrDefaultAsync(cancellationToken);

        if (notification is null)
        {
            throw new ApiException("Notification not found", $"Notification with id {id} was not found.",
                StatusCodes.Status404NotFound);
        }

        return Ok(notification);
    }

    /// <summary>
    /// Sends a test notification to the current user through the portal channel, so that an administrator can
    /// check the delivery without setting up a notification template.
    /// </summary>
    /// <param name="request">Subject and text of the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The notification as the current user receives it.</returns>
    [HttpPost("create-test-notification")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType<UserNotificationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserNotificationDto>> CreateTestNotificationAsync(
        [FromBody] CreateTestNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Message))
            throw new ApiException("Invalid notification request", "Subject and message are required.",
                StatusCodes.Status400BadRequest);

        var userId = await GetCurrentUserIdAsync(cancellationToken);

        var portalChannelCode = typeof(PortalChannel).FullName!;
        var channel = await context.NotificationChannels
                          .FirstOrDefaultAsync(x => x.Code == portalChannelCode, cancellationToken)
                      ?? throw new ApiException("Notification channel not found",
                          "The portal notification channel is not registered.", StatusCodes.Status404NotFound);

        var notificationType = await context.NotificationTypes
                                   .FirstOrDefaultAsync(x => x.Name == TestNotificationType, cancellationToken)
                               ?? context.NotificationTypes.Add(new NotificationTypeEntity
                               {
                                   Name = TestNotificationType,
                                   Description = "Test notifications sent by administrators to themselves.",
                                   Scope = typeof(NotificationController).Assembly.GetName().Name!
                               }).Entity;

        var sentAt = DateTimeOffset.UtcNow;
        var notificationUser = new NotificationUserEntity
        {
            UserId = userId,
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            Importance = ImportanceLevel.Low,
            NotificationChannelId = channel.NotificationChannelId,
            SentAt = sentAt
        };
        context.Notifications.Add(new NotificationEntity
        {
            NotificationType = notificationType,
            CreatedAt = sentAt,
            NotificationUsers = [notificationUser]
        });
        await context.SaveChangesAsync(cancellationToken);

        // The notification is stored either way; the live delivery reaches the open portal pages of the user.
        if (userCache.Users.TryGetValue(userId, out var user))
            channelService.Notify(portalChannelCode, user, notificationUser.Subject, notificationUser.Message,
                notificationUser.Importance);

        return await GetNotificationByIdAsync(notificationUser.NotificationUserId, cancellationToken);
    }

    private async Task<int> GetCurrentUserIdAsync(CancellationToken cancellationToken)
    {
        var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!string.IsNullOrWhiteSpace(keycloakId))
        {
            var user = await userDirectory.GetUserByKeycloakIdAsync(keycloakId, cancellationToken);
            if (user is not null)
            {
                return user.UserId;
            }
        }

        var email = currentClaimService.GetUserEmail();
        if (!string.IsNullOrWhiteSpace(email))
        {
            var user = await userDirectory.GetUserByEmailAsync(email, cancellationToken);
            if (user is not null)
            {
                return user.UserId;
            }
        }

        throw new ApiException("User not found", "Current user was not found in the user directory.",
            StatusCodes.Status404NotFound);
    }
}
