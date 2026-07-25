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
    /// Retrieves the current SPA proxy configuration for the application.
    /// </summary>
    [HttpGet("get-spa-proxy-settings")]
    public IActionResult GetSpaProxySettings()
    {
        var isStandalone = appSettings.ServiceAddingMode == AddingMode.Local;
        var mainTarget = appSettings.Services.Oip;

        if (isStandalone)
        {
            return Ok(new[]
            {
                CreateWsProxy(
                    ["/hubs/notification"],
                    mainTarget),
                CreateKeepAliveProxy(
                    [
                        "/manifest.json",
                        "/api",
                        "/signin-oidc",
                        "/signout-callback-oidc",
                        "/signout-oidc",
                        "/swagger",
                        "/health",
                        "/metrics"
                    ],
                    mainTarget)
            });
        }

        return Ok(new[]
        {
            CreateKeepAliveProxy(
                ["/api/users", "/api/user-profile"],
                appSettings.Services.OipUsers),
            CreateWsProxy(
                ["/hubs/notification", "/api/notification"],
                appSettings.Services.OipNotifications),
            CreateWsProxy(
                ["/api/discussion"],
                appSettings.Services.OipDiscussions),
            CreateWsProxy(
                ["/api/applications"],
                appSettings.Services.OipApplications),
            CreateKeepAliveProxy(
                [
                    "/manifest.json",
                    "/api",
                    "/signin-oidc",
                    "/signout-callback-oidc",
                    "/signout-oidc",
                    "/swagger",
                    "/health",
                    "/metrics"
                ],
                mainTarget)
        });
    }

    private static ProxyConfigEntry CreateKeepAliveProxy(string[] context, string target) =>
        new()
        {
            Context = context,
            Target = target,
            Secure = false,
            Ws = true,
            Headers = new ProxyHeaders
            {
                Connection = "Keep-Alive"
            }
        };

    private static ProxyConfigEntry CreateWsProxy(string[] context, string target) =>
        new()
        {
            Context = context,
            Target = target,
            Secure = false,
            ChangeOrigin = true,
            Ws = true
        };

    public sealed class ProxyConfigEntry
    {
        public required string[] Context { get; init; }

        public required string Target { get; init; }

        public bool Secure { get; init; }

        public bool ChangeOrigin { get; init; }

        public bool Ws { get; init; }

        public ProxyHeaders? Headers { get; init; }
    }

    public sealed class ProxyHeaders
    {
        public required string Connection { get; init; }
    }
}
