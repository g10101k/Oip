using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Oip.Api.Controllers.Api;
using Oip.Base.Extensions;
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
[Route("api/security")]
[ApiExplorerSettings(GroupName = "base")]
public class SecurityController(
    IBaseOipModuleAppSettings appSettings,
    KeycloakService keycloakService,
    IAntiforgery antiforgery) : ControllerBase
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

    [HttpPost("create-auth-session")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public IActionResult CreateAuthSession()
    {
        var redirectUri = GetAuthRedirectUri(Request.Headers.Referer.FirstOrDefault());
        if (string.IsNullOrWhiteSpace(redirectUri))
            redirectUri = "/";

        return Challenge(new AuthenticationProperties { RedirectUri = redirectUri },
            OipModuleApplication.OpenIdConnectAuthenticationScheme);
    }

    private static string? GetAuthRedirectUri(string? referer)
    {
        if (string.IsNullOrWhiteSpace(referer))
            return null;

        if (!Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
            return referer;

        var query = QueryHelpers.ParseQuery(refererUri.Query);
        var returnUrl = query.TryGetValue("returnUrl", out var values)
            ? values.FirstOrDefault()
            : null;

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
        return SignOut(
            new AuthenticationProperties { RedirectUri = "/unauthorized" },
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
}
