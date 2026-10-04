namespace Oip.Hitl.Base.Workflows;

/// <summary>
/// Step being performed by <see cref="AutomatedStepDefinition{TResult}.RunAsync"/>.
/// </summary>
/// <param name="WorkflowId">Id of the workflow.</param>
/// <param name="StepId">Id of the step, e.g. to keep its files in <see cref="WorkflowFileStorage"/>.</param>
public record AutomatedStepContext(string WorkflowId, string StepId);
