namespace Oip.Hil.Base.Workflows;

/// <summary>
/// Describes a step the workflow performs without a user, for example an activity call. The step is shown in the
/// task list next to the user steps; its page renders it read-only. Applications define their own steps and pass
/// them to <see cref="UserWorkflowBase.AutomatedStepAsync{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">Type of the step result.</typeparam>
public abstract class AutomatedStepDefinition<TResult> : StepDefinition
{
    /// <summary>
    /// Performs the step. Runs in the workflow context, so it must be deterministic: call activities for any I/O.
    /// </summary>
    /// <param name="context">Workflow and id of the step being performed.</param>
    public abstract Task<TResult> RunAsync(AutomatedStepContext context);

    /// <summary>
    /// Returns who performed the step, shown as the step completer, or <c>null</c>.
    /// </summary>
    public virtual string? GetPerformer(TResult result) => null;

    /// <summary>
    /// Returns the files of the result attached to the step.
    /// </summary>
    public virtual IReadOnlyList<WorkflowAttachment> GetAttachments(TResult result) => [];
}
