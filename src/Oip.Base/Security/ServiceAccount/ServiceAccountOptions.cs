using Oip.Base.Helpers;
using Oip.Base.Settings;

namespace Oip.Base.Security.ServiceAccount;

/// <summary>
/// Connection parameters used to obtain a service account token with the OAuth 2.0 Client Credentials grant.
/// </summary>
public sealed class ServiceAccountOptions
{
    /// <summary>
    /// Absolute URL of the token endpoint.
    /// </summary>
    public required string TokenEndpoint { get; init; }

    /// <summary>
    /// Client identifier of the service account.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Client secret of the service account.
    /// </summary>
    public required string ClientSecret { get; init; }

    /// <summary>
    /// Accept any server certificate of the token endpoint. Intended for development only.
    /// </summary>
    public bool AcceptAnyServerCertificate { get; init; }

    /// <summary>
    /// Creates options for the Keycloak client described by the security service settings.
    /// </summary>
    /// <param name="settings">Security service settings.</param>
    /// <param name="isDevelopment">Whether the application runs in the development environment.</param>
    public static ServiceAccountOptions FromSecurityService(SecurityServiceSettings settings, bool isDevelopment)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new ServiceAccountOptions
        {
            TokenEndpoint = (settings.DockerUrl ?? settings.BaseUrl)
                .UrlAppend("realms")
                .UrlAppend(settings.Realm)
                .UrlAppend("protocol/openid-connect/token"),
            ClientId = settings.ClientId,
            ClientSecret = settings.ClientSecret,
            AcceptAnyServerCertificate = isDevelopment
        };
    }
}
