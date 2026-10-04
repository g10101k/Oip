using Oip.Hitl.Base.Agents;
using Temporalio.Exceptions;

namespace Oip.Hitl.Services;

/// <summary>
/// User token of the agent run for the tools of Oip.Hitl itself, read from <see cref="AgentUserTokenStore"/>.
/// </summary>
public class LocalAgentUserTokenProvider(AgentUserTokenStore tokenStore) : IAgentUserTokenProvider
{
    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(string? audience = null,
        CancellationToken cancellationToken = default)
    {
        var runId = AgentRun.GetRunId();
        var token = await tokenStore.GetAccessTokenAsync(runId, audience, cancellationToken);
        return token?.AccessToken ?? throw new ApplicationFailureException(
            "The token of the user is not available: the user has signed out or the chat did not pass it",
            GrpcAgentUserTokenProvider.UserTokenUnavailableError, nonRetryable: true);
    }
}
