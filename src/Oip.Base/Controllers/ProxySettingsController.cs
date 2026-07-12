using Microsoft.AspNetCore.Mvc;
using Oip.Base.Settings;

namespace Oip.Base.Controllers;

/// <summary>
/// Controller responsible for managing security-related operations,
/// including role retrieval and Keycloak client configuration.
/// </summary>
[ApiController]
[Route("api/proxy-settings")]
[ApiExplorerSettings(GroupName = "ignore")]
public class ProxySettingsController(ISettings appSettings) : ControllerBase
{
    /// <summary>
    /// Retrieves the current proxy configuration settings for the application.
    /// </summary>
    [HttpGet("get-spa-proxy-settings")]
    public IActionResult GetSpaProxySettings()
    {
        var config = new
        {
            Standalone = appSettings.ServiceAddingMode == AddingMode.Local,
            Targets = new
            {
                Main = appSettings.Services.Oip,
                Applications = appSettings.ServiceAddingMode == AddingMode.Local ? appSettings.Services.Oip : appSettings.Services.OipApplications,
                Users = appSettings.ServiceAddingMode == AddingMode.Local ? appSettings.Services.Oip : appSettings.Services.OipUsers,
                Discussion = appSettings.ServiceAddingMode == AddingMode.Local ? appSettings.Services.Oip : appSettings.Services.OipDiscussions,
                Notification = appSettings.ServiceAddingMode == AddingMode.Local ? appSettings.Services.Oip : appSettings.Services.OipNotifications
            }
        };

        return Ok(config);
    }
}
