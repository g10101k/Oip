using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers.Api;
using Oip.Base.Helpers;
using Oip.Base.Settings;

namespace Oip.Base.Controllers;

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
    private readonly IBaseOipModuleAppSettings _appSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityController"/> class.
    /// </summary>
    /// <param name="appSettings">Application settings used for Keycloak configuration.</param>
    public SecurityController(IBaseOipModuleAppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    /// <summary>
    /// Retrieves Keycloak client settings needed by frontend applications.
    /// </summary>
    /// <remarks>
    /// This endpoint is publicly accessible and provides client configuration such as authority URL,
    /// client ID, scopes, and secure routes for frontend OAuth2/OIDC initialization.
    /// </remarks>
    /// <returns>
    /// A <see cref="GetKeycloakClientSettingsResponse"/> object containing frontend configuration.
    /// </returns>
    [HttpGet("get-keycloak-client-settings")]
    [AllowAnonymous]
    public GetKeycloakClientSettingsResponse GetKeycloakClientSettings()
    {
        var securitySettings = _appSettings.SecurityService;

        HashSet<string> securityRoutes =
        [
            _appSettings.OipUrls
        ];
        foreach (var q in securitySettings.Front.SecureRoutes)
        {
            securityRoutes.Add(q);
        }

        return new GetKeycloakClientSettingsResponse()
        {
            // Use base url from settings
            Authority = securitySettings.BaseUrl.UrlAppend("realms").UrlAppend(securitySettings.Realm),
            ClientId = securitySettings.Front.ClientId,
            Scope = securitySettings.Front.Scope,
            ResponseType = securitySettings.Front.ResponseType,
            SilentRenew = securitySettings.Front.SilentRenew,
            UseRefreshToken = securitySettings.Front.UseRefreshToken,
            LogLevel = securitySettings.Front.LogLevel,
            SecureRoutes = securityRoutes.ToList()
        };
    }
}