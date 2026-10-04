using Temporalio.Activities;
using Temporalio.Exceptions;

namespace Oip.Hitl.Base.Agents;

/// <summary>
/// Agent run of the current tool activity.
/// </summary>
public static class AgentRun
{
    /// <summary>
    /// Returns the id of the agent run: the workflow id of the current activity.
    /// </summary>
    /// <exception cref="ApplicationFailureException">The activity is not called by a workflow.</exception>
    public static string GetRunId() =>
        ActivityExecutionContext.Current.Info.WorkflowId ??
        throw new ApplicationFailureException("The tool is not called by an agent run", nonRetryable: true);
}
