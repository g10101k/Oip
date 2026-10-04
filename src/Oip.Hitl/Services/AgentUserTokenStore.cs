using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Oip.Hitl.Services;

/// <summary>
/// Access token of the user of an agent run.
/// </summary>
/// <param name="AccessToken">The token.</param>
/// <param name="ExpiresAt">When the token expires.</param>
public record AgentUserToken(string AccessToken, DateTimeOffset ExpiresAt);

/// <summary>
/// User tokens of the agent runs, so tools call services with the rights of the user who started the run. When a run
/// starts, the access token of the user is exchanged for a refresh token of the gateway client in the same Keycloak
/// session; it is kept protected until the run ends, never in the workflow history. Access tokens are refreshed from
/// it on demand and optionally narrowed to the audience of the called service. When the user signs out, the session
/// ends and the run has no user token any more.
/// </summary>
public class AgentUserTokenStore(
    IAgentUserTokenStorage storage,
    KeycloakTokenClient keycloak,
    IDataProtectionProvider dataProtectionProvider,
    TimeProvider timeProvider,
    ILogger<AgentUserTokenStore> logger)
{
    private const string ProtectorPurpose = "Oip.Hitl.AgentUserToken";

    // An access token is refreshed this long before it expires, so it does not expire while it is used.
    private static readonly TimeSpan RenewBeforeExpiration = TimeSpan.FromSeconds(30);

    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);

    /// <summary>
    /// Exchanges the access token of the user for the token of the run and keeps it until the run ends.
    /// </summary>
    /// <param name="runId">The agent run.</param>
    /// <param name="accessToken">Access token of the user who started the run.</param>
    /// <param name="lifetime">How long the token is kept, at least as long as the run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether the token is kept; <c>false</c> when Keycloak refused to exchange it.</returns>
    public async Task<bool> StoreAsync(string runId, string accessToken, TimeSpan lifetime,
        CancellationToken cancellationToken)
    {
        KeycloakTokenResponse token;
        try
        {
            token = await keycloak.ExchangeAsync(accessToken, withRefreshToken: true, audience: null,
                cancellationToken);
        }
        catch (Exception e) when (e is KeycloakTokenException or HttpRequestException)
        {
            logger.LogWarning("The user token of agent run {RunId} is not exchanged, so its tools get no user " +
                              "token: {Error}", runId, e.Message);
            return false;
        }

        if (token.RefreshToken is null)
        {
            logger.LogWarning("Keycloak returned no refresh token for agent run {RunId}: allow refresh tokens in " +
                              "the standard token exchange of the client", runId);
            return false;
        }

        var now = timeProvider.GetUtcNow();
        await SaveAsync(runId, new StoredToken(token.RefreshToken, token.AccessToken,
            now.AddSeconds(token.ExpiresIn), now + lifetime));
        return true;
    }

    /// <summary>
    /// Returns an access token of the user of the run, refreshed when it expires.
    /// </summary>
    /// <param name="runId">The agent run.</param>
    /// <param name="audience">Client the token is narrowed to; <c>null</c> for the token of the gateway client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The token; <c>null</c> when the run has no token or the session of the user has ended.</returns>
    public async Task<AgentUserToken?> GetAccessTokenAsync(string runId, string? audience,
        CancellationToken cancellationToken)
    {
        var stored = await LoadAsync(runId);
        if (stored is null) return null;

        var now = timeProvider.GetUtcNow();
        try
        {
            if (stored.AccessTokenExpiresAt - RenewBeforeExpiration <= now)
            {
                var refreshed = await keycloak.RefreshAsync(stored.RefreshToken, cancellationToken);
                stored = stored with
                {
                    RefreshToken = refreshed.RefreshToken ?? stored.RefreshToken,
                    AccessToken = refreshed.AccessToken,
                    AccessTokenExpiresAt = now.AddSeconds(refreshed.ExpiresIn)
                };
                await SaveAsync(runId, stored);
            }

            if (string.IsNullOrWhiteSpace(audience))
                return new AgentUserToken(stored.AccessToken, stored.AccessTokenExpiresAt);

            var narrowed = await keycloak.ExchangeAsync(stored.AccessToken, withRefreshToken: false, audience,
                cancellationToken);
            return new AgentUserToken(narrowed.AccessToken, now.AddSeconds(narrowed.ExpiresIn));
        }
        catch (KeycloakTokenException e) when ((int)e.StatusCode is >= 400 and < 500)
        {
            logger.LogWarning("The user token of agent run {RunId} is not available: {Error}", runId, e.Message);
            return null;
        }
    }

    /// <summary>
    /// Deletes the token of the run, e.g. when the run has ended.
    /// </summary>
    public Task DeleteAsync(string runId) => storage.DeleteAsync(runId);

    private async Task<StoredToken?> LoadAsync(string runId)
    {
        var value = await storage.GetAsync(runId);
        return value is null ? null : JsonSerializer.Deserialize<StoredToken>(_protector.Unprotect(value));
    }

    private Task SaveAsync(string runId, StoredToken token) =>
        storage.SetAsync(runId, _protector.Protect(JsonSerializer.Serialize(token)), token.ExpiresAt);

    private sealed record StoredToken(
        string RefreshToken,
        string AccessToken,
        DateTimeOffset AccessTokenExpiresAt,
        DateTimeOffset ExpiresAt);
}
