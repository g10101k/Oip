using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oip.Hitl.Base.Settings;
using Oip.Hitl.Base.Workflows;

namespace Oip.Hitl.Base.Agents;

/// <summary>
/// Registers the tools of the skill worker in Oip.Hitl when the worker starts, retrying until Oip.Hitl is reachable,
/// so skills can give them to agents.
/// </summary>
public class AgentToolRegistrationService(
    GrpcAgentService.GrpcAgentServiceClient client,
    TemporalSettings temporalSettings,
    OipWorkflowOptions workflowOptions,
    ILogger<AgentToolRegistrationService> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tools = AgentToolDescriber.Describe(workflowOptions.Activities, temporalSettings.TaskQueue);
        var request = new RegisterToolsRequest { TaskQueue = temporalSettings.TaskQueue };
        request.Tools.AddRange(tools.Select(x => new AgentTool
        {
            Name = x.Name,
            Description = x.Description,
            ParametersSchema = x.ParametersSchema.GetRawText(),
            ActivityName = x.ActivityName,
            HasArguments = x.HasArguments,
            TimeoutSeconds = x.TimeoutSeconds,
            MaxAttempts = x.MaxAttempts,
            RequiresApproval = x.RequiresApproval
        }));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await client.RegisterToolsAsync(request, cancellationToken: stoppingToken);
                logger.LogInformation("Agent tools of task queue {TaskQueue} registered: {Tools}",
                    request.TaskQueue, string.Join(", ", tools.Select(x => x.Name)));
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (RpcException e) when (e.StatusCode == StatusCode.InvalidArgument)
            {
                // E.g. a tool name is taken by another worker: retrying does not help.
                logger.LogError("Agent tools of task queue {TaskQueue} are rejected: {Error}", request.TaskQueue,
                    e.Status.Detail);
                return;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to register agent tools, retrying in {Delay}", RetryDelay);
                try
                {
                    await Task.Delay(RetryDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }
}
