using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Api.Controllers.Api;
using Oip.Base.Exceptions;
using Oip.Base.Helpers;
using Oip.Base.Settings;

namespace Oip.Api.Controllers;

/// <summary>
/// Controller responsible for managing security-related operations,
/// including role retrieval and Keycloak client configuration.
/// </summary>
[ApiController]
[Route("api/security")]
[ApiExplorerSettings(GroupName = "base")]
public class SecurityController(IBaseOipModuleAppSettings appSettings) : ControllerBase
{
    /// <summary>
    /// Retrieves Keycloak client settings needed by frontend applications.
    /// </summary>
    /// <returns>
    /// A <see cref="GetKeycloakClientSettingsResponse"/> object containing frontend configuration.
    /// </returns>
    [HttpGet("get-keycloak-client-settings")]
    [AllowAnonymous]
    [ProducesResponseType<GetKeycloakClientSettingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public GetKeycloakClientSettingsResponse GetKeycloakClientSettings()
    {
        var securitySettings = appSettings.SecurityService;

        HashSet<string> securityRoutes = new();
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