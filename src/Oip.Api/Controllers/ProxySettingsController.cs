using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Api.Controllers.Api;
using Oip.Base.Exceptions;
using Oip.Base.Helpers;
using Oip.Base.Services;
using Oip.Base.Settings;
using Oip.Data.Constants;

namespace Oip.Api.Controllers;

/// <summary>
/// Controller responsible for managing security-related operations,
/// including role retrieval and Keycloak client configuration.
/// </summary>
[ApiController]
[Route("api/proxy-settings")]
[ApiExplorerSettings(GroupName = "ignore")]
public class ProxySettingsController(IBaseOipModuleAppSettings appSettings) : ControllerBase
{
    /// <summary>
    /// Retrieves the current proxy configuration settings for the application.
    /// </summary>
    [HttpGet("get-spa-proxy-settings")]
    public IActionResult GetSpaProxySettings()
    {
        var config = new
        {
            Targets = new
            {
                Main = appSettings.Services.Oip,
                Users = appSettings.IsStandalone ? appSettings.Services.Oip : appSettings.Services.OipUsers,
                Discussion = appSettings.IsStandalone ? appSettings.Services.Oip : appSettings.Services.OipDiscussions,
                Notification = appSettings.IsStandalone
                    ? appSettings.Services.Oip
                    : appSettings.Services.OipNotifications
            }
        };

        return Ok(config);
    }
}