using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Api;
using Oip.Base.Services;
using Oip.Controllers.Api;
using Oip.Settings;

namespace Oip.Controllers;

/// <summary>
/// Security controller
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
    [Authorize(Roles = "admin")]
    [HttpGet("get-realm-roles")]
    public async Task<IActionResult> GetRealmRoles()
    {
        var realmRoles = await _keycloakService.GetRealmRoles();
        return Ok(realmRoles.Select(x => x.Name));
    }

    /// <summary> 
    /// Get keycloak client settings
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-keycloak-client-settings")]
    public async Task<IActionResult> GetKeycloakClientSettings()
    {
        var securitySettings = AppSettings.Instance.SecurityService;
        return Ok(new GetKeycloakClientSettingsResponse()
        {
            Authority = $"{securitySettings.BaseUrl}realms/{securitySettings.Realm}",
            ClientId = securitySettings.Front.ClientId,
            Scope = securitySettings.Front.Scope,
            ResponseType = securitySettings.Front.ResponseType,
            SilentRenew = securitySettings.Front.SilentRenew,
            UseRefreshToken = securitySettings.Front.UseRefreshToken,
            LogLevel = securitySettings.Front.LogLevel,
        });
    }
}