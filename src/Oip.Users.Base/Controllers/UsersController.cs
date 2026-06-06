using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oip.Data.Constants;
using Oip.Users.Base.Data.Entities;
using Oip.Users.Base.Data.Repositories;
using Oip.Users.Base.Notifications;
using Oip.Users.Base.Services;

namespace Oip.Users.Base.Controllers;

/// <summary>
/// Controller for managing user entities
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController(
    UserRepository userRepository,
    UserSyncService userSyncService,
    IServiceScopeFactory scopeFactory,
    INotificationPublisher notificationPublisher,
    ILogger<UsersController> logger)
    : ControllerBase
{
    /// <summary>
    /// Gets a list of users with pagination
    /// </summary>
    /// <param name="skip">Number of records to skip</param>
    /// <param name="take">Number of records to take</param>
    /// <returns>List of users</returns>
    [HttpGet("get-all-users")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task<ActionResult<IEnumerable<UserEntity>>> GetAllUsers([FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        var users = await userRepository.GetAllAsync(skip, take);
        return Ok(users);
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User entity</returns>
    [HttpGet("get-user")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task<ActionResult<UserEntity>> GetUser([FromQuery] int id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Gets a user by Keycloak ID
    /// </summary>
    /// <param name="keycloakId">Keycloak user ID</param>
    /// <returns>User entity</returns>
    [HttpGet("get-user-by-keycloak-id")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task<ActionResult<UserEntity>> GetUserByKeycloakId([FromQuery] string keycloakId)
    {
        var user = await userRepository.GetByKeycloakIdAsync(keycloakId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Searches users by search term
    /// </summary>
    /// <param name="term">Search term</param>
    /// <returns>List of users matching the search term</returns>
    [HttpGet("search-user")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task<ActionResult<IEnumerable<UserEntity>>> SearchUsers([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
        {
            return BadRequest("Search term must be at least 2 characters long");
        }

        var users = await userRepository.SearchAsync(term);
        return Ok(users);
    }

    /// <summary>
    /// Synchronizes a user from Keycloak
    /// </summary>
    /// <param name="request">Request with Keycloak user ID</param>
    /// <returns>Accepted status</returns>
    [HttpPost("sync-user")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task<ActionResult> SyncUser([FromBody] SyncUserRequest request)
    {
        try
        {
            await userSyncService.SyncUserFromKeycloak(request.KeycloakUserId);
            return Accepted();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error syncing user {KeycloakUserId}", request.KeycloakUserId);
            return StatusCode(500, "Error syncing user");
        }
    }

    /// <summary>
    /// Starts synchronization of all users from Keycloak
    /// </summary>
    /// <returns>Accepted status with message</returns>
    [HttpPost("sync-all-users")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task<ActionResult> SyncAllUsers()
    {
        try
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var scopedUserSyncService = scope.ServiceProvider.GetRequiredService<UserSyncService>();
                    await scopedUserSyncService.SyncAllUsersAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error syncing all users in background");
                }
            });

            return Accepted(new { message = "Full synchronization started" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting full synchronization");
            return StatusCode(500, "Error starting synchronization");
        }
    }

    [HttpPost("custom-notification")]
    [Authorize]
    public async Task<ActionResult> CustomNotify(CustomUserNotify notify)
    {
        await notificationPublisher.Notify(notify);
        return Ok();
    }
}

/// <summary>
/// Represents a request to synchronize a user from Keycloak
/// </summary>
/// <param name="KeycloakUserId">The unique identifier of the user in Keycloak</param>
public record SyncUserRequest(string KeycloakUserId);
