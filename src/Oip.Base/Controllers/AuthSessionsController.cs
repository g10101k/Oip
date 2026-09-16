using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Exceptions;
using Oip.Base.Services;

namespace Oip.Base.Controllers;

/// <summary>
/// Lets administrators inspect and terminate active authentication sessions.
/// </summary>
[ApiController]
[Route("api/auth-sessions")]
[ApiExplorerSettings(GroupName = "base")]
[Authorize(Roles = SecurityConstants.AdminRole)]
public class AuthSessionsController(IAuthSessionStore sessionStore) : ControllerBase
{
    /// <summary>
    /// Returns active sessions, optionally limited to one user.
    /// </summary>
    [HttpGet("get-auth-sessions")]
    [ProducesResponseType<List<AuthSessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<List<AuthSessionDto>> GetAuthSessions([FromQuery] string? userId,
        CancellationToken cancellationToken)
    {
        var currentKey = GetCurrentSessionKey();
        var sessions = await sessionStore.GetSessionsAsync(cancellationToken);

        return sessions
            .Where(x => string.IsNullOrEmpty(userId) || string.Equals(x.UserId, userId, StringComparison.Ordinal))
            .OrderByDescending(x => x.LastActivityUtc)
            .Select(x => new AuthSessionDto
            {
                SessionId = x.SessionId,
                UserId = x.UserId,
                UserName = x.UserName,
                DisplayName = x.DisplayName,
                Email = x.Email,
                CreatedUtc = x.CreatedUtc,
                LastActivityUtc = x.LastActivityUtc,
                ExpiresUtc = x.ExpiresUtc,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                IsCurrent = x.Key == currentKey
            })
            .ToList();
    }

    /// <summary>
    /// Terminates a single session. The caller's own session must be closed through sign out.
    /// </summary>
    [HttpDelete("delete-auth-session/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAuthSession(string sessionId, CancellationToken cancellationToken)
    {
        var sessions = await sessionStore.GetSessionsAsync(cancellationToken);
        var session = sessions.FirstOrDefault(x => string.Equals(x.SessionId, sessionId, StringComparison.Ordinal))
                      ?? throw new ApiException("Session not found",
                          $"Session '{sessionId}' was not found.", StatusCodes.Status404NotFound);

        if (session.Key == GetCurrentSessionKey())
            throw new ApiException("Current session",
                "The current session cannot be terminated here. Use sign out instead.",
                StatusCodes.Status400BadRequest);

        await sessionStore.RemoveAsync(session.Key, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Terminates every session of a user except the caller's own one.
    /// </summary>
    [HttpDelete("delete-auth-sessions-by-user/{userId}")]
    [ProducesResponseType<DeleteAuthSessionsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<DeleteAuthSessionsResponse> DeleteAuthSessionsByUser(string userId,
        CancellationToken cancellationToken)
    {
        var currentKey = GetCurrentSessionKey();
        var sessions = await sessionStore.GetSessionsAsync(cancellationToken);
        var deleted = 0;

        foreach (var session in sessions.Where(x =>
                     string.Equals(x.UserId, userId, StringComparison.Ordinal) && x.Key != currentKey))
        {
            await sessionStore.RemoveAsync(session.Key, cancellationToken);
            deleted++;
        }

        return new DeleteAuthSessionsResponse { DeletedCount = deleted };
    }

    /// <summary>
    /// Terminates every session except the caller's own one.
    /// </summary>
    [HttpDelete("delete-all-auth-sessions")]
    [ProducesResponseType<DeleteAuthSessionsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<DeleteAuthSessionsResponse> DeleteAllAuthSessions(CancellationToken cancellationToken)
    {
        var currentKey = GetCurrentSessionKey();
        var sessions = await sessionStore.GetSessionsAsync(cancellationToken);
        var deleted = 0;

        foreach (var session in sessions.Where(x => x.Key != currentKey))
        {
            await sessionStore.RemoveAsync(session.Key, cancellationToken);
            deleted++;
        }

        return new DeleteAuthSessionsResponse { DeletedCount = deleted };
    }

    private string? GetCurrentSessionKey() => User.FindFirstValue(AuthSessionMetadata.SessionKeyClaimType);
}
