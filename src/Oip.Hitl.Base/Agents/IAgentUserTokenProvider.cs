namespace Oip.Hitl.Base.Agents;

/// <summary>
/// Access token of the user who started the agent run, for tools that call services with the rights of the user.
/// Called from a tool activity: the run is the workflow of the activity. When the run has no user token, e.g. the
/// gateway could not exchange it, a non-retryable failure is thrown, so the model gets the error.
/// </summary>
public interface IAgentUserTokenProvider
{
    /// <summary>
    /// Returns an access token of the user of the current agent run.
    /// </summary>
    /// <param name="audience">Keycloak client the token is narrowed to; <c>null</c> for the token of the gateway
    /// client, which the services of OIP accept.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<string> GetAccessTokenAsync(string? audience = null, CancellationToken cancellationToken = default);
}
