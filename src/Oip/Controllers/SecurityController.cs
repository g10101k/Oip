using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Api;
using Oip.Base.Services;
using Oip.Controllers.Api;

namespace Oip.Controllers;

/// <summary>
/// Module federation controller
/// </summary>
[ApiController]
[Route("api/security")]
public class SecurityController : ControllerBase
{
    private readonly KeycloakService _keycloakService;

    /// <summary>.ctor</summary>
    public SecurityController(ILogger<SecurityController> logger, KeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-realm-roles")]
    public async Task<IActionResult> GetRealmRoles()
    {
        var realmRoles = await _keycloakService.GetRealmRoles();
        return Ok(realmRoles.Select(x => x.Name));
    }
}