using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Entities;
using Oip.Base.Exceptions;

namespace Oip.Base.Controllers;

/// <summary>
/// Manages the assets loaded by the OIP shell.
/// </summary>
[Authorize]
[Route("api/customization")]
[ApiExplorerSettings(GroupName = "base")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
public class CustomizationController(OipModuleContext context) : ControllerBase
{
    /// <summary>
    /// Returns the current site customization for an administrator.
    /// </summary>
    [HttpGet("get-customization")]
    [ProducesResponseType<SiteCustomization>(StatusCodes.Status200OK)]
    public async Task<SiteCustomization> GetCustomization(CancellationToken cancellationToken)
    {
        RequireAdministrator();
        var customization = await SiteCustomizationQuery().SingleAsync(cancellationToken);
        return new SiteCustomization
        {
            Css = customization.Css,
            JavaScript = customization.JavaScript,
            FaviconBase64 = customization.Favicon is null ? null : Convert.ToBase64String(customization.Favicon)
        };
    }

    /// <summary>
    /// Replaces the site-wide CSS, JavaScript, and optional favicon.
    /// </summary>
    [HttpPut("update-customization")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCustomization([FromBody] SiteCustomization? request,
        CancellationToken cancellationToken)
    {
        RequireAdministrator();
        if (request is null || !ModelState.IsValid)
            throw new ApiException("Invalid customization",
                "Provide CSS and JavaScript up to 512 Ki characters each and a favicon up to 256 KiB.",
                StatusCodes.Status400BadRequest);

        var favicon = DecodeFavicon(request.FaviconBase64);
        await SiteCustomizationQuery().ExecuteUpdateAsync(updates => updates
            .SetProperty(x => x.Css, request.Css)
            .SetProperty(x => x.JavaScript, request.JavaScript)
            .SetProperty(x => x.Favicon, favicon), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Serves the stylesheet referenced by the shell index page.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("get-customer-css")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ContentResult> GetCustomerCss(CancellationToken cancellationToken)
    {
        var css = await SiteCustomizationQuery().Select(x => x.Css).SingleAsync(cancellationToken);
        Response.Headers.XContentTypeOptions = "nosniff";
        return Content(css, "text/css; charset=utf-8");
    }

    /// <summary>
    /// Serves the script referenced by the shell index page.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("get-customer-script")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ContentResult> GetCustomerScript(CancellationToken cancellationToken)
    {
        var script = await SiteCustomizationQuery().Select(x => x.JavaScript).SingleAsync(cancellationToken);
        Response.Headers.XContentTypeOptions = "nosniff";
        return Content(script, "text/javascript; charset=utf-8");
    }

    /// <summary>
    /// Serves the custom favicon or redirects to the default favicon.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("get-favicon")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetFavicon(CancellationToken cancellationToken)
    {
        var favicon = await SiteCustomizationQuery().Select(x => x.Favicon).SingleAsync(cancellationToken);
        if (favicon is null)
            return LocalRedirect("~/assets/favicon.svg");

        Response.Headers.XContentTypeOptions = "nosniff";
        return File(favicon, GetFaviconContentType(favicon) ?? "application/octet-stream");
    }

    private void RequireAdministrator()
    {
        if (!User.IsInRole(SecurityConstants.AdminRole))
            throw new ApiException("Forbidden", "An administrator role is required to manage customization.",
                StatusCodes.Status403Forbidden);
    }

    private IQueryable<CustomizationEntity> SiteCustomizationQuery() =>
        context.Customizations.AsNoTracking().Where(x => x.CustomizationId == CustomizationEntity.SiteId);

    private static byte[]? DecodeFavicon(string? base64)
    {
        if (base64 is null)
            return null;

        byte[] data;
        try
        {
            data = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            throw new ApiException("Invalid favicon", "The favicon must be Base64-encoded.",
                StatusCodes.Status400BadRequest);
        }

        if (data.Length > SiteCustomization.MaxFaviconBytes || GetFaviconContentType(data) is null)
            throw new ApiException("Invalid favicon", "Upload a PNG or ICO image up to 256 KiB.",
                StatusCodes.Status400BadRequest);

        return data;
    }

    private static string? GetFaviconContentType(ReadOnlySpan<byte> data)
    {
        if (data.Length >= 33 && data.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            return "image/png";
        if (data.Length >= 22 && data.StartsWith(new byte[] { 0, 0, 1, 0 }))
            return "image/x-icon";
        return null;
    }
}
