namespace Oip.Hil.Controllers.Api;

/// <summary>
/// Result of starting the user task demo workflow.
/// </summary>
public class RunUserTaskDemoResponse
{
    /// <summary>
    /// Workflow id.
    /// </summary>
    public required string WorkflowInstanceId { get; set; }

    /// <summary>
    /// Id of the first step, if it was created in time.
    /// </summary>
    public string? StepId { get; set; }

    /// <summary>
    /// Link to the page of the first step.
    /// </summary>
    public string? StepUrl { get; set; }
}

/// <summary>
/// Result of a workflow run.
/// </summary>
public class RunWorkflowResponse
{
    /// <summary>
    /// Workflow id.
    /// </summary>
    public required string WorkflowInstanceId { get; set; }

    /// <summary>
    /// Temporal workflow status.
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Value returned by the workflow.
    /// </summary>
    public string? Result { get; set; }
}
