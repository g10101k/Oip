using Oip.Base.Exceptions;
using Oip.Hitl.Services;
using Temporalio.Activities;
using Temporalio.Exceptions;

namespace Oip.Hitl.Workflows.Activities;

/// <summary>
/// Temporal activities that read agents and skills.
/// </summary>
public class AgentActivities(AgentService agentService)
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
}
