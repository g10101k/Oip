using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Exceptions;
using Oip.Base.Extensions;
using Oip.Base.Security.DefaultSecrets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oip.Base.Services;

namespace Oip.Base.Controllers;

/// <summary>
/// Controller responsible for managing security-related operations.
/// </summary>
[ApiController]
[Route("api/security")]
[ApiExplorerSettings(GroupName = "base")]
public class SecurityController(
    KeycloakService keycloakService,
    IAntiforgery antiforgery,
    DefaultSecretsValidator defaultSecretsValidator,
    IHostEnvironment environment,
    ILogger<SecurityController> logger) : ControllerBase
{
    [HttpGet("get-current-auth-session")]
    [AllowAnonymous]
    [ProducesResponseType<AuthSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    public ActionResult<AuthSessionResponse> GetCurrentAuthSession()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Unauthorized(new ApiExceptionResponse("Unauthorized", "Authentication session is required.",
                StatusCodes.Status401Unauthorized));

        return new AuthSessionResponse
        {
            IsAuthenticated = true,
            UserName = User.FindFirstValue("preferred_username") ?? User.Identity?.Name,
            DisplayName = User.FindFirstValue("name"),
            Email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email"),
            Roles = User.Claims
                .Where(claim => claim.Type == ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Distinct()
                .ToList()
        };
    }

    /// <summary>
    /// Starts the Keycloak sign-in and returns the browser to the requested page of the application afterwards.
    /// </summary>
    /// <param name="returnUrl">
    /// Local path to open after signing in. Passed explicitly because the page address the browser sends as
    /// Referer may already be reset by the client router when the form is submitted; the Referer query is
    /// only a fallback.
    /// </param>
    [HttpPost("create-auth-session")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public IActionResult CreateAuthSession([FromQuery] string? returnUrl = null)
    {
        var referer = Request.Headers.Referer.FirstOrDefault();
        var redirectUri = GetAuthRedirectUri(referer, returnUrl);
        if (string.IsNullOrWhiteSpace(redirectUri))
            redirectUri = "/";

        logger.LogInformation(
            "Creating auth session. ReturnUrl: {ReturnUrl}; Referer: {Referer}; RedirectUri: {RedirectUri}",
            returnUrl, referer, redirectUri);

        return Challenge(new AuthenticationProperties { RedirectUri = redirectUri },
            OipModuleApplication.OpenIdConnectAuthenticationScheme);
    }

    /// <summary>
    /// Builds the address to return to after signing in: the local return path on the origin of the page that
    /// started the sign-in, which differs from the backend origin when the client is served separately.
    /// </summary>
    private static string? GetAuthRedirectUri(string? referer, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(referer))
            return IsLocalReturnUrl(returnUrl) ? returnUrl : null;

        if (!Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
            return referer;

        if (!IsLocalReturnUrl(returnUrl))
        {
            var query = QueryHelpers.ParseQuery(refererUri.Query);
            returnUrl = query.TryGetValue("returnUrl", out var values)
                ? values.FirstOrDefault()
                : null;
        }

        if (!IsLocalReturnUrl(returnUrl))
            return referer;

        return $"{refererUri.Scheme}://{refererUri.Authority}{returnUrl}";
    }

    private static bool IsLocalReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return false;

        return returnUrl[0] == '/'
               && (returnUrl.Length == 1 || returnUrl[1] != '/' && returnUrl[1] != '\\')
               && !returnUrl.Contains('\r')
               && !returnUrl.Contains('\n');
    }

    [HttpPost("delete-auth-session")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public IActionResult DeleteAuthSession()
    {
        var redirectUri = GetLogoutRedirectUri(Request.Headers.Referer.FirstOrDefault());

        return SignOut(
            new AuthenticationProperties { RedirectUri = redirectUri },
            OipModuleApplication.CookieAuthenticationScheme,
            OipModuleApplication.OpenIdConnectAuthenticationScheme);
    }

    [HttpGet("get-auth-csrf-token")]
    [Authorize]
    [ProducesResponseType<AuthCsrfTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    public AuthCsrfTokenResponse GetAuthCsrfToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return new AuthCsrfTokenResponse
        {
            Token = tokens.RequestToken ?? string.Empty,
            HeaderName = OipModuleApplication.CsrfHeaderName
        };
    }

    /// <summary>
    /// Retrieves all realm roles from Keycloak.
    /// </summary>
    /// <returns>
    /// A list of role names as <see cref="string"/>.
    /// </returns>
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [HttpGet("get-realm-roles")]
    public async Task<IEnumerable<string>> GetRealmRoles()
    {
        var realmRoles = await keycloakService.GetRealmRoles();
        return realmRoles.Select(x => x.Name).ToList();
    }
        
    /// <summary>
    /// Retrieves the settings that still hold a secret shipped with the repository.
    /// </summary>
    /// <returns>
    /// A <see cref="DefaultSecretsReportResponse"/> listing every setting that needs attention.
    /// </returns>
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [HttpGet("get-default-secrets-report")]
    [ProducesResponseType<DefaultSecretsReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public DefaultSecretsReportResponse GetDefaultSecretsReport() =>
        DefaultSecretsReportResponse.Create(defaultSecretsValidator.Report, environment.IsProduction());

    private static string GetLogoutRedirectUri(string? referer)
    {
        if (!string.IsNullOrWhiteSpace(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
            return $"{refererUri.Scheme}://{refererUri.Authority}/unauthorized";

        return "/unauthorized";
    }
}
