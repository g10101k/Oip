using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Entities;
using Oip.Base.Exceptions;
using Oip.Base.Helpers;

namespace Oip.Base.Controllers;

/// <summary>
/// Manages the assets loaded by the OIP shell.
/// </summary>
[Route("api/customization")]
[Authorize(Roles = SecurityConstants.AdminRole)]
[ApiExplorerSettings(GroupName = "base")]
[ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
public class CustomizationController(OipModuleContext context) : ControllerBase
{
    private const string SvgContentType = "image/svg+xml";

    /// <summary>
    /// Returns the current site customization for an administrator.
    /// </summary>
    [HttpGet("get-customization")]
    [ProducesResponseType<SiteCustomization>(StatusCodes.Status200OK)]
    public async Task<SiteCustomization> GetCustomization(CancellationToken cancellationToken)
    {
        var customization = await SiteCustomizationQuery().SingleAsync(cancellationToken);
        return new SiteCustomization
        {
            Css = customization.Css,
            JavaScript = customization.JavaScript,
            FaviconBase64 = customization.Favicon is null ? null : Convert.ToBase64String(customization.Favicon),
            LogoBase64 = customization.Logo is null ? null : Convert.ToBase64String(customization.Logo)
        };
    }

    /// <summary>
    /// Updates the site-wide CSS, JavaScript, favicon, and logo.
    /// </summary>
    [HttpPut("update-customization")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCustomization([FromBody] SiteCustomization? request,
        CancellationToken cancellationToken)
    {
        if (request is null || !ModelState.IsValid)
            throw new ApiException("Invalid customization",
                "Provide CSS and JavaScript up to 512 Ki characters each and images up to 256 KiB each.",
                StatusCodes.Status400BadRequest);

        var favicon = DecodeFavicon(request.FaviconBase64);
        var logo = request.LogoBase64 is null ? null : CustomizationLogoImage.Decode(request.LogoBase64);
        await SiteCustomizationQuery().ExecuteUpdateAsync(updates =>
        {
            updates.SetProperty(x => x.Css, request.Css)
                .SetProperty(x => x.JavaScript, request.JavaScript)
                .SetProperty(x => x.Favicon, favicon)
                .SetProperty(x => x.Logo, logo);
        }, cancellationToken);

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

        return CustomizationImageFile(favicon, GetFaviconContentType(favicon) ?? "application/octet-stream");
    }

    /// <summary>
    /// Serves the custom logo used by the shell.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("get-logo")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetLogo(CancellationToken cancellationToken)
    {
        var logo = await SiteCustomizationQuery().Select(x => x.Logo).SingleAsync(cancellationToken);
        if (logo is null)
            return NoContent();

        return CustomizationImageFile(logo,
            CustomizationLogoImage.HasPngSignature(logo) ? "image/png" : SvgContentType);
    }

    private FileContentResult CustomizationImageFile(byte[] bytes, string contentType)
    {
        Response.Headers.XContentTypeOptions = "nosniff";
        if (contentType == SvgContentType)
            Response.Headers.ContentSecurityPolicy = CustomizationLogoImage.SvgContentSecurityPolicy;
        return File(bytes, contentType);
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
            throw new ApiException("Invalid favicon", "Upload a PNG, ICO, or static SVG image up to 256 KiB.",
                StatusCodes.Status400BadRequest);

        return data;
    }

    private static string? GetFaviconContentType(byte[] data)
    {
        if (data.Length >= 33 && CustomizationLogoImage.HasPngSignature(data))
            return "image/png";
        if (data.Length >= 22 && data.AsSpan().StartsWith(new byte[] { 0, 0, 1, 0 }))
            return "image/x-icon";
        return CustomizationLogoImage.IsStaticSvg(data) ? SvgContentType : null;
    }
}