using Oip.Hil.Base.Workflows;
using Oip.Hil.Base.Workflows.Steps;
using Temporalio.Workflows;

namespace Oip.Hil.Workflows;

/// <summary>
/// Demo workflow: waits for a user to approve or reject a task on the Angular page. "Approved" continues with a form
/// on its own page, "Rejected" ends the workflow right away.
/// </summary>
[Workflow]
public class UserTaskDemoWorkflow : UserWorkflowBase
{
    private const string Approved = "Approved";
    private const string Rejected = "Rejected";

    /// <summary>
    /// Runs the workflow.
    /// </summary>
    [WorkflowRun]
    public async Task RunAsync()
    {
        var task = await UserStepAsync(new UserTaskStep
        {
            Title = "Approve the request",
            Description = "Demo task created by UserTaskDemoWorkflow. Choose an outcome to resume the workflow.",
            Outcomes = [Approved, Rejected]
        });

        if (task.Result == Approved)
        {
            Workflow.Logger.LogInformation("APPROVED by {User}: {Comment}", task.CompletedBy, task.Comment);

            var form = await UserStepAsync(new UserFormStep
            {
                Title = "Approval details",
                Description = "Fill in the details of the approved request.",
                Fields = ["Budget", "Deadline"]
            });
            Workflow.Logger.LogInformation("FORM submitted by {User}: {Values}", form.CompletedBy,
                string.Join(", ", form.Result.Select(x => $"{x.Key}={x.Value}")));
        }
        else
        {
            Workflow.Logger.LogInformation("REJECTED by {User}: {Comment}", task.CompletedBy, task.Comment);
        }

        Workflow.Logger.LogInformation("Workflow finished.");
    }
}
