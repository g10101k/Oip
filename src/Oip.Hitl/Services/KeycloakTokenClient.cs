using System.Net;
using System.Text.Json.Serialization;
using Oip.Base.Security.ServiceAccount;

namespace Oip.Hitl.Services;

/// <summary>
/// Token response of Keycloak.
/// </summary>
/// <param name="AccessToken">Access token.</param>
/// <param name="ExpiresIn">Lifetime of the access token in seconds.</param>
/// <param name="RefreshToken">Refresh token, when issued.</param>
public record KeycloakTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken);

/// <summary>
/// Keycloak rejected a token request, e.g. because the session of the user has ended.
/// </summary>
/// <param name="statusCode">HTTP status of the response.</param>
/// <param name="message">Error of the response.</param>
public class KeycloakTokenException(HttpStatusCode statusCode, string message) : Exception(message)
{
    /// <summary>
    /// HTTP status of the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; } = statusCode;
}

/// <summary>
/// Token requests of the gateway client (<c>SecurityService:ClientId</c>) to Keycloak: standard token exchange and
/// refresh.
/// </summary>
public class KeycloakTokenClient(IHttpClientFactory httpClientFactory, ServiceAccountOptions options)
{
    /// <summary>
    /// Name of the HTTP client used to call the token endpoint.
    /// </summary>
    public const string HttpClientName = "Oip.AgentUserToken";

    private const string TokenExchangeGrant = "urn:ietf:params:oauth:grant-type:token-exchange";
    private const string AccessTokenType = "urn:ietf:params:oauth:token-type:access_token";
    private const string RefreshTokenType = "urn:ietf:params:oauth:token-type:refresh_token";

    /// <summary>
    /// Exchanges the access token of a user for a token of the gateway client in the same session of the user.
    /// Keycloak requires the gateway client to be in the audience of the token.
    /// </summary>
    /// <param name="subjectToken">Access token of the user.</param>
    /// <param name="withRefreshToken">Whether a refresh token is requested too; the client must allow it.</param>
    /// <param name="audience">Client the token is narrowed to; <c>null</c> to not narrow it.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<KeycloakTokenResponse> ExchangeAsync(string subjectToken, bool withRefreshToken, string? audience,
        CancellationToken cancellationToken)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = TokenExchangeGrant,
            ["subject_token"] = subjectToken,
            ["subject_token_type"] = AccessTokenType,
            ["requested_token_type"] = withRefreshToken ? RefreshTokenType : AccessTokenType
        };
        if (!string.IsNullOrWhiteSpace(audience)) form["audience"] = audience;
        return RequestAsync(form, cancellationToken);
    }

    /// <summary>
    /// Gets a new access token with the refresh token.
    /// </summary>
    public Task<KeycloakTokenResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken) =>
        RequestAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        }, cancellationToken);

    private async Task<KeycloakTokenResponse> RequestAsync(Dictionary<string, string> form,
        CancellationToken cancellationToken)
    {
        form["client_id"] = options.ClientId;
        form["client_secret"] = options.ClientSecret;
        using var content = new FormUrlEncodedContent(form);
        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.PostAsync(options.TokenEndpoint, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new KeycloakTokenException(response.StatusCode,
                $"Keycloak rejected the {form["grant_type"]} request ({(int)response.StatusCode}): " +
                await response.Content.ReadAsStringAsync(cancellationToken));

        return await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken)
               ?? throw new KeycloakTokenException(response.StatusCode, "Keycloak returned no token");
    }
}
