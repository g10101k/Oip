using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Constants;
using Oip.Users.Services;

namespace Oip.Users.Controllers;

/// <summary>
/// Controller responsible for managing security-related operations,
/// including role retrieval and Keycloak client configuration.
/// </summary>
/// <remarks>
/// Provides endpoints for administrators to retrieve Keycloak realm roles,
/// as well as public access to frontend configuration for OAuth2 client setup.
/// </remarks>
[ApiController]
[Route("api/security")]
[ApiExplorerSettings(GroupName = "base")]
public class SecurityController : ControllerBase
{
    private readonly KeycloakService _keycloakService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityController"/> class.
    /// </summary>
    /// <param name="keycloakService">Service to interact with Keycloak.</param>
    public SecurityController(KeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    /// <summary>
    /// Retrieves all realm roles from Keycloak.
    /// </summary>
    /// <remarks>
    /// This endpoint is restricted to administrators.
    /// Useful for role management in the application UI or backend.
    /// </remarks>
    /// <returns>
    /// A list of role names as <see cref="string"/>.
    /// </returns>
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [HttpGet("get-realm-roles")]
    public async Task<IEnumerable<string>> GetRealmRoles()
    {
        var realmRoles = await _keycloakService.GetRealmRoles();
        return realmRoles.Select(x => x.Name).ToList();
    }
}