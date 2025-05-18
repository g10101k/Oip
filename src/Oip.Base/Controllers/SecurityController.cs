using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Api;
using Oip.Base.Helpers;
using Oip.Base.Services;
using Oip.Base.Settings;

namespace Oip.Base.Controllers;

/// <summary>
/// Security controller
/// </summary>
[ApiController]
[Route("api/security")]
public class SecurityController : ControllerBase
{
    private readonly KeycloakService _keycloakService;
    private readonly IBaseOipModuleAppSettings _appSettings;

    /// <summary>.ctor</summary>
    public SecurityController(KeycloakService keycloakService, IBaseOipModuleAppSettings appSettings)
    {
        _keycloakService = keycloakService;
        _appSettings = appSettings;
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
        var securitySettings = _appSettings.SecurityService;

        HashSet<string> securityRoutes = new HashSet<string>();
        securityRoutes.Add(_appSettings.OipUrls);
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