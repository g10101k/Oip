using System.Text.Json;
using Grpc.Core;
using Oip.Base.Exceptions;
using Oip.Hitl.Base.Agents;
using Oip.Hitl.Services;
using Oip.Hitl.Workflows;
using Temporalio.Api.Enums.V1;
using Temporalio.Client;
using RpcException = Grpc.Core.RpcException;

namespace Oip.Hitl.Grpc;

/// <summary>
/// Agent runtime for skill workers: registers their tools and gives their tool activities the token of the user of
/// the agent run. Called with the service account token.
/// </summary>
public class AgentGrpcService(
    AgentService agentService,
    AgentUserTokenStore tokenStore,
    ITemporalClient client) : GrpcAgentService.GrpcAgentServiceBase
{
    /// <inheritdoc />
    public override async Task<RegisterToolsResponse> RegisterTools(RegisterToolsRequest request,
        ServerCallContext context)
    {
        var tools = request.Tools.Select(x => new AgentToolDefinition(x.Name, x.Description, ParseSchema(x),
            x.ActivityName, x.HasArguments, request.TaskQueue, x.TimeoutSeconds, x.MaxAttempts,
            x.RequiresApproval)).ToList();
        try
        {
            await agentService.RegisterToolsAsync(request.TaskQueue, tools, context.CancellationToken);
        }
        catch (ApiException e)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, e.Message));
        }

        return new RegisterToolsResponse();
    }

    /// <inheritdoc />
    public override async Task<GetUserTokenResponse> GetUserToken(GetUserTokenRequest request,
        ServerCallContext context)
    {
        // Only for running agent runs, so a token is not given out after the run has ended.
        if (!request.RunId.StartsWith(AgentWorkflow.RunIdPrefix, StringComparison.Ordinal) ||
            await GetStatusAsync(request.RunId, context.CancellationToken) is not { } status)
            throw new RpcException(new Status(StatusCode.NotFound, $"Agent run {request.RunId} not found"));
        if (status != WorkflowExecutionStatus.Running)
            throw new RpcException(new Status(StatusCode.FailedPrecondition,
                $"Agent run {request.RunId} is not running"));

        var token = await tokenStore.GetAccessTokenAsync(request.RunId,
            request.HasAudience ? request.Audience : null, context.CancellationToken);
        if (token is null)
            throw new RpcException(new Status(StatusCode.FailedPrecondition,
                "The token of the user is not available: the user has signed out or the chat did not pass it"));

        return new GetUserTokenResponse
        {
            AccessToken = token.AccessToken,
            ExpiresIn = (int)Math.Max(0, (token.ExpiresAt - DateTimeOffset.UtcNow).TotalSeconds)
        };
    }

    private async Task<WorkflowExecutionStatus?> GetStatusAsync(string runId, CancellationToken cancellationToken)
    {
        try
        {
            var execution = await client.GetWorkflowHandle(runId).DescribeAsync(
                new WorkflowDescribeOptions { Rpc = new RpcOptions { CancellationToken = cancellationToken } });
            return execution.Status;
        }
        catch (Temporalio.Exceptions.RpcException e) when (e.Code is Temporalio.Exceptions.RpcException.StatusCode.NotFound)
        {
            return null;
        }
    }

    private static JsonElement ParseSchema(AgentTool tool)
    {
        try
        {
            using var schema = JsonDocument.Parse(tool.ParametersSchema);
            return schema.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Parameters schema of tool {tool.Name} is not valid JSON"));
        }
    }
}
