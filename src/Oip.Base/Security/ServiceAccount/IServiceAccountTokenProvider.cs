namespace Oip.Base.Security.ServiceAccount;

/// <summary>
/// Provides access tokens of the service account for calls between OIP services.
/// </summary>
public interface IServiceAccountTokenProvider
{
    /// <summary>
    /// Returns a valid access token, requesting a new one when the cached token is missing or about to expire.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Drops the cached token if it is still the specified one, so the next call requests a new token.
    /// </summary>
    /// <param name="accessToken">The token that was rejected by the server.</param>
    void Invalidate(string accessToken);
}
