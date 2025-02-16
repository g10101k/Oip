using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Helpers;
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
    public SecurityController(KeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    /// <summary> 
    /// Get all roles
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "admin")]
    [HttpGet("get-realm-roles")]
    public async Task<IEnumerable<string>> GetRealmRoles()
    {
        var realmRoles = await _keycloakService.GetRealmRoles();
        return realmRoles.Select(x => x.Name);
    }

    /// <summary> 
    /// Get keycloak client settings
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-keycloak-client-settings")]
    [AllowAnonymous]
    public GetKeycloakClientSettingsResponse GetKeycloakClientSettings()
    {
        var securitySettings = AppSettings.Instance.SecurityService;

        HashSet<string> securityRoutes = new HashSet<string>();
        securityRoutes.Add(AppSettings.Instance.OipUrls);
        foreach (var q in securitySettings.Front.SecureRoutes)
        {
            securityRoutes.Add(q);
        }

        return new GetKeycloakClientSettingsResponse()
        {
            Authority = securitySettings.BaseUrl.UrlAppend("realms").UrlAppend(securitySettings.Realm),
            ClientId = securitySettings.Front.ClientId,
            Scope = securitySettings.Front.Scope,
            ResponseType = securitySettings.Front.ResponseType,
            SilentRenew = securitySettings.Front.SilentRenew,
            UseRefreshToken = securitySettings.Front.UseRefreshToken,
            LogLevel = securitySettings.Front.LogLevel,
            SecureRoutes = securityRoutes.ToList(),
        };
    }
}