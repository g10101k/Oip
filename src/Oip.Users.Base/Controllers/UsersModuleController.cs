using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;
using Oip.Base.Security;
using Oip.Users.Base.Controllers.Api;
using Oip.Users.Base.Data.Repositories;
using Oip.Users.Base.Services;

namespace Oip.Users.Base.Controllers;

/// <summary>
/// Module that lists the users synchronized from Keycloak and starts their full synchronization.
/// </summary>
[ApiController]
[Authorize]
[Route("api/users-module")]
[ApiExplorerSettings(GroupName = "users")]
public class UsersModuleController(
    UserRepository userRepository,
    UsersSyncCoordinator syncCoordinator,
    ModuleRepository moduleRepository)
    : BaseModuleController<object>(moduleRepository)
{
    private const int MaxTake = 1000;

    /// <inheritdoc />
    public override string? Icon => "pi pi-users";

    /// <summary>
    /// Returns a page of the users whose e-mail, first name or last name contains the search text.
    /// </summary>
    /// <param name="request">Page and search text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Right(SecurityConstants.Read), HttpPost("get-users-by-filter")]
    [ProducesResponseType<UsersPageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UsersPageDto>> GetUsersByFilter([FromBody] GetUsersByFilterRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Skip < 0)
            throw new ApiException("Validation error", "The number of users to skip must not be negative",
                StatusCodes.Status400BadRequest);
        if (request.Take is < 1 or > MaxTake)
            throw new ApiException("Validation error", $"The number of users to return must be from 1 to {MaxTake}",
                StatusCodes.Status400BadRequest);

        var (users, totalCount) = await userRepository.GetByFilterAsync(request.Search, request.IsActive,
            request.Skip, request.Take, cancellationToken);

        return Ok(new UsersPageDto(
            users.Select(u => new UserListItemDto(u.UserId, u.Email, u.FirstName, u.LastName, u.IsActive,
                u.LastSyncedAt)).ToList(),
            totalCount));
    }

    /// <summary>
    /// Returns the number of all, active and inactive users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Right(SecurityConstants.Read), HttpGet("get-users-summary")]
    [ProducesResponseType<UsersSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UsersSummaryDto>> GetUsersSummary(CancellationToken cancellationToken)
    {
        var (totalCount, activeCount) = await userRepository.GetActivityCountsAsync(cancellationToken);
        return Ok(new UsersSummaryDto(totalCount, activeCount, totalCount - activeCount));
    }

    /// <summary>
    /// Starts the full synchronization of users with Keycloak in the background.
    /// </summary>
    /// <returns>The state of the started synchronization.</returns>
    [Right(SecurityConstants.Edit), HttpPost("sync-all-users")]
    [ProducesResponseType<UsersSyncStatus>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status409Conflict)]
    public ActionResult<UsersSyncStatus> SyncAllUsers()
    {
        if (!syncCoordinator.TryStart(out var status))
            throw new ApiException("Synchronization is running",
                "The synchronization of users with Keycloak is already running", StatusCodes.Status409Conflict);

        return Accepted(status);
    }

    /// <summary>
    /// Returns the state of the current or the last full synchronization of users with Keycloak.
    /// </summary>
    [Right(SecurityConstants.Read), HttpGet("get-sync-status")]
    [ProducesResponseType<UsersSyncStatus>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public ActionResult<UsersSyncStatus> GetSyncStatus()
    {
        return Ok(syncCoordinator.Status);
    }

    /// <inheritdoc />
    public override List<SecurityResponse> GetModuleRights()
    {
        return
        [
            new SecurityResponse
            {
                Code = SecurityConstants.Read,
                Name = "Read",
                Description = "Can view the synchronized users",
                Roles = [SecurityConstants.AdminRole]
            },
            new SecurityResponse
            {
                Code = SecurityConstants.Edit,
                Name = "Edit",
                Description = "Can start the synchronization of users with Keycloak",
                Roles = [SecurityConstants.AdminRole]
            }
        ];
    }
}
