using System.Net.Http.Json;
using Oip.Base.Clients;

namespace Oip.Base.Security.ServiceAccount;

/// <summary>
/// Obtains service account tokens with the OAuth 2.0 Client Credentials grant and caches them until expiration.
/// </summary>
public sealed class ServiceAccountTokenProvider(
    IHttpClientFactory httpClientFactory,
    ServiceAccountOptions options,
    TimeProvider timeProvider) : IServiceAccountTokenProvider, IDisposable
{
    /// <summary>
    /// Name of the HTTP client used to call the token endpoint.
    /// </summary>
    public const string HttpClientName = "Oip.ServiceAccount";

    /// <summary>
    /// How long before expiration the cached token is renewed.
    /// </summary>
    private static readonly TimeSpan RenewBeforeExpiration = TimeSpan.FromSeconds(30);

    private readonly SemaphoreSlim _lock = new(1, 1);
    private CachedToken? _token;

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = _token;
        if (token is not null && token.RenewAt > timeProvider.GetUtcNow())
            return token.AccessToken;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            token = _token;
            if (token is not null && token.RenewAt > timeProvider.GetUtcNow())
                return token.AccessToken;

            token = await RequestTokenAsync(cancellationToken);
            _token = token;
            return token.AccessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public void Invalidate(string accessToken)
    {
        var token = _token;
        if (token is not null && token.AccessToken == accessToken)
            Interlocked.CompareExchange(ref _token, null, token);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _lock.Dispose();
    }

    private async Task<CachedToken> RequestTokenAsync(CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = options.ClientId,
            ["client_secret"] = options.ClientSecret
        });

        var client = httpClientFactory.CreateClient(HttpClientName);
        var requestedAt = timeProvider.GetUtcNow();
        using var response = await client.PostAsync(options.TokenEndpoint, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Failed to obtain a service account token for client '{options.ClientId}' " +
                $"({(int)response.StatusCode} {response.StatusCode}): {error}");
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken);
        if (string.IsNullOrEmpty(authResponse?.AccessToken))
            throw new InvalidOperationException(
                $"Token endpoint returned no access token for client '{options.ClientId}'.");

        var lifetime = TimeSpan.FromSeconds(Math.Max(0, authResponse.ExpiresIn));
        var renewBefore = TimeSpan.FromTicks(Math.Min(RenewBeforeExpiration.Ticks, lifetime.Ticks / 2));
        return new CachedToken(authResponse.AccessToken, requestedAt + lifetime - renewBefore);
    }

    private sealed record CachedToken(string AccessToken, DateTimeOffset RenewAt);
}
