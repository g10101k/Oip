using Grpc.Core;
using Temporalio.Exceptions;
using RpcException = Grpc.Core.RpcException;

namespace Oip.Hitl.Base.Agents;

/// <summary>
/// Gets the user token of the agent run from Oip.Hitl, for the tools of skill workers.
/// </summary>
public class GrpcAgentUserTokenProvider(GrpcAgentService.GrpcAgentServiceClient client) : IAgentUserTokenProvider
{
    /// <summary>
    /// Error type of the failure when the run has no user token.
    /// </summary>
    public const string UserTokenUnavailableError = "UserTokenUnavailable";

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(string? audience = null,
        CancellationToken cancellationToken = default)
    {
        var request = new GetUserTokenRequest { RunId = AgentRun.GetRunId() };
        if (!string.IsNullOrWhiteSpace(audience)) request.Audience = audience;
        try
        {
            var response = await client.GetUserTokenAsync(request, cancellationToken: cancellationToken);
            return response.AccessToken;
        }
        catch (RpcException e) when (e.StatusCode is StatusCode.NotFound or StatusCode.FailedPrecondition)
        {
            throw new ApplicationFailureException(e.Status.Detail, UserTokenUnavailableError, nonRetryable: true);
        }
    }
}
