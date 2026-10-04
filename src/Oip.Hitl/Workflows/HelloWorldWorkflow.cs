using Temporalio.Workflows;

namespace Oip.Hitl.Workflows;

/// <summary>
/// Simplest Temporal workflow: writes two lines to the log and returns a greeting.
/// </summary>
[Workflow]
public class HelloWorldWorkflow
{
    private const string Greeting = "Hello from Temporal!";

    /// <summary>
    /// Runs the workflow.
    /// </summary>
    [WorkflowRun]
    public Task<string> RunAsync()
    {
        Workflow.Logger.LogInformation(Greeting);
        Workflow.Logger.LogInformation("Workflow finished.");
        return Task.FromResult(Greeting);
    }
}
