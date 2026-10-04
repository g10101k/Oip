using Oip.Base.Exceptions;
using Oip.Hitl.Services;
using Temporalio.Activities;
using Temporalio.Exceptions;

namespace Oip.Hitl.Workflows.Activities;

/// <summary>
/// Request to publish a user step of an agent run.
/// </summary>
/// <param name="StreamKey">Key of the <see cref="AgentEventStream"/> of the run.</param>
/// <param name="StepId">Step of the workflow.</param>
/// <param name="Kind">Kind of the step, see <see cref="AgentStepKind"/>.</param>
public record PublishUserStepRequest(string StreamKey, string StepId, string Kind);

/// <summary>
/// Temporal activities that read agents and skills and notify the gateway about the run.
/// </summary>
public class AgentActivities(AgentService agentService, AgentEventStream eventStream)
{
    /// <summary>
    /// Loads a skill of an agent. A skill that is not available to the agent fails without retries.
    /// </summary>
    [Activity]
    public async Task<LoadedSkill> LoadSkillAsync(LoadSkillRequest request)
    {
        try
        {
            return await agentService.LoadSkillAsync(request, ActivityExecutionContext.Current.CancellationToken);
        }
        catch (ApiException e)
        {
            throw new ApplicationFailureException(e.Message, e, e.Title, nonRetryable: true);
        }
    }

    /// <summary>
    /// Tells the gateway streaming the run that the run waits for the user to complete the step.
    /// </summary>
    [Activity]
    public Task PublishUserStepAsync(PublishUserStepRequest request) =>
        eventStream.PublishUserStepAsync(request.StreamKey, request.StepId, request.Kind);
}
