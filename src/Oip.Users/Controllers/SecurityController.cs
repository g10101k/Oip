using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Data.Constants;
using Oip.Users.Services;

namespace Oip.Users.Controllers;

/// <summary>
/// Controller responsible for managing security-related operations,
/// including role retrieval and Keycloak client configuration.
/// </summary>
[ApiController]
[Route("api/security")]
[ApiExplorerSettings(GroupName = "base")]
public class SecurityController(KeycloakService keycloakService) : ControllerBase
{
    /// <summary>
    /// Retrieves all realm roles from Keycloak.
    /// </summary>
    /// <returns>
    /// A list of role names as <see cref="string"/>.
    /// </returns>
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [HttpGet("get-realm-roles")]
    public async Task<IEnumerable<string>> GetRealmRoles()
    {
        var realmRoles = await keycloakService.GetRealmRoles();
        return realmRoles.Select(x => x.Name).ToList();
    }
}