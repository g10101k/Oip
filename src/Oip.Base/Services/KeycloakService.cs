using Microsoft.Extensions.Logging;
using Oip.Base.Clients;
using Oip.Base.Settings;

namespace Oip.Base.Services;

/// <summary>
/// Provides services for interacting with Keycloak identity management system.
/// </summary>
public class KeycloakService(KeycloakClient client, ILogger<KeycloakService> logger, SecurityServiceSettings _options)
{
    private bool _isAuthenticated;


    /// <summary>
    /// Asynchronously retrieves a user by their ID.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user if found, or null if not found.</returns>
    public async Task<UserRepresentation?> GetUserAsync(string userId)
    {
        await EnsureAuthenticatedAsync();
        return await client.GetUserAsync(_options.Realm, userId);
    }

    /// <summary>
    /// Asynchronously retrieves a list of users with pagination support.
    /// </summary>
    /// <param name="offset">The offset for pagination (default is 0).</param>
    /// <param name="limit">The maximum number of users to retrieve (default is 100).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of users.</returns>
    public async Task<IEnumerable<UserRepresentation>> GetUsersAsync(int offset = 0, int limit = 100)
    {
        await EnsureAuthenticatedAsync();
        var users = await client.GetUsersAsync(_options.Realm, first: offset, max: limit);
        return users ?? [];
    }


    /// <summary>
    /// Ensure authentication with Keycloak
    /// </summary>
    private async Task EnsureAuthenticatedAsync()
    {
        if (!_isAuthenticated)
        {
            try
            {
                await client.AuthenticateAsync(_options.ClientId, _options.ClientSecret, _options.Realm);
                _isAuthenticated = true;
                logger.LogInformation("Successfully authenticated with Keycloak");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to authenticate with Keycloak");
                throw;
            }
        }
    }

    /// <summary>
    /// Asynchronously retrieves the total count of users from the Keycloak identity management system.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of users.</returns>
    public async Task<int> GetUsersCountAsync()
    {
        try
        {
            await EnsureAuthenticatedAsync();
            return await client.GetUsersCountAsync(_options.Realm);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting users count from Keycloak");
            throw;
        }
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    /// <returns></returns>
    public async Task<List<Role>> GetRealmRoles()
    {
        _ = await client.Authentication(_options.ClientId, _options.ClientSecret, _options.Realm,
            CancellationToken.None);

        return await client.GetRoles(_options.Realm, CancellationToken.None);
    }
}