using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Settings;

namespace Oip.Api.Controllers;

/// <summary>
/// API controller for frontend application registry.
/// </summary>
[ApiController]
[Route("api/applications")]
[ApiExplorerSettings(GroupName = "base")]
public class ApplicationsController(IBaseOipModuleAppSettings appSettings) : ControllerBase
{
    /// <summary>
    /// Retrieves registered frontend applications.
    /// </summary>
    [Authorize]
    [HttpGet("get")]
    public IEnumerable<ApplicationRegistryItemDto> Get()
    {
        return appSettings.ApplicationRegistry.Applications
            .Where(application => application.Enabled)
            .OrderBy(application => application.Order)
            .ThenBy(application => application.DisplayName)
            .Select(application => new ApplicationRegistryItemDto
            {
                Code = application.Code,
                DisplayName = application.DisplayName,
                BaseUrl = NormalizeUrl(application.BaseUrl),
                ApiBaseUrl = NormalizeUrl(application.ApiBaseUrl),
                Icon = application.Icon,
                Order = application.Order,
                IsCurrent = string.Equals(
                    application.Code,
                    appSettings.ApplicationRegistry.CurrentApplicationCode,
                    StringComparison.OrdinalIgnoreCase)
            });
    }

    private static string NormalizeUrl(string url)
    {
        return url.TrimEnd('/');
    }
}

/// <summary>
/// Frontend application registry item.
/// </summary>
public class ApplicationRegistryItemDto
{
    /// <summary>
    /// Stable application code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable application name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Frontend application URL.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Backend API URL.
    /// </summary>
    public string ApiBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// PrimeIcons CSS class.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Display order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Indicates whether this application is current.
    /// </summary>
    public bool IsCurrent { get; set; }
}
