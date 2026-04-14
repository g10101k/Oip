using Microsoft.AspNetCore.Mvc;
using Oip.Base.Settings;

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