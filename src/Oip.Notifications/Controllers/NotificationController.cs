using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Exceptions;
using Oip.Base.Services;
using Oip.Notifications.Contracts;
using Oip.Notifications.Data.Contexts;
using Oip.Users.Base;

namespace Oip.Notifications.Controllers;

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
    UserService currentUserService) : ControllerBase
{
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
            .CountAsync(x => x.UserId == userId, cancellationToken);

        return Ok(new UserNotificationCountResponse(count));
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

        var email = currentUserService.GetUserEmail();
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
