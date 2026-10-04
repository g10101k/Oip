using System.Text.Json;
using Oip.Hitl.Services;
using Oip.Hitl.Workflows.Activities;
using Temporalio.Common;
using Temporalio.Workflows;

namespace Oip.Hitl.Workflows;

/// <summary>
/// Input of <see cref="AgentWorkflow"/>.
/// </summary>
/// <param name="ProviderId">Provider that answers.</param>
/// <param name="Messages">Chat history; the agent answers the last message.</param>
/// <param name="Settings">Request parameters sent to the chat completions API as is, e.g. <c>temperature</c>.</param>
/// <param name="StreamKey">Key of the <see cref="AgentEventStream"/> the answer is streamed to; <c>null</c> to not stream.</param>
/// <param name="UserName">Login of the user who sent the message.</param>
public record AgentWorkflowInput(
    int ProviderId,
    IReadOnlyList<AgentMessage> Messages,
    IReadOnlyDictionary<string, JsonElement>? Settings,
    string? StreamKey,
    string? UserName);

/// <summary>
/// Agent run started by the OpenAI-compatible gateway for a chat message: the model answers the chat and the
/// answer is streamed to the gateway through <see cref="AgentWorkflowInput.StreamKey"/>.
/// </summary>
[Workflow]
public class AgentWorkflow
{
    /// <summary>
    /// Runs the workflow and returns the answer of the agent.
    /// </summary>
    [WorkflowRun]
    public Task<AgentTurnResult> RunAsync(AgentWorkflowInput input)
    {
        Workflow.Logger.LogInformation("Agent run for {User}: {Count} message(s)", input.UserName,
            input.Messages.Count);

        var request = new AgentTurnRequest(input.ProviderId, input.Messages, input.Settings, input.StreamKey);
        return Workflow.ExecuteActivityAsync((LlmActivities activities) => activities.ChatTurnAsync(request),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromMinutes(5),
                // A few heartbeat intervals of the activity.
                HeartbeatTimeout = TimeSpan.FromSeconds(30),
                RetryPolicy = new RetryPolicy { MaximumAttempts = 3 }
            });
    }
}
