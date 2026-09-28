using Oip.Hil.Base.Workflows;
using Oip.Hil.Base.Workflows.Steps;
using Oip.Hil.Workflows.Steps;
using Temporalio.Exceptions;
using Temporalio.Workflows;

namespace Oip.Hil.Workflows;

/// <summary>
/// Demo workflow: waits for a user to approve or reject a task on the Angular page. "Approved" continues with a form
/// on its own page, then an LLM assesses the form and either approves the request or escalates it to a user;
/// "Rejected" ends the workflow right away.
/// </summary>
[Workflow]
public class UserTaskDemoWorkflow : UserWorkflowBase
{
    private const string Approved = "Approved";
    private const string Rejected = "Rejected";
    private const string Escalated = "Escalated";

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

            await AssessAsync(task.Comment, form.Result);
        }
        else
        {
            Workflow.Logger.LogInformation("REJECTED by {User}: {Comment}", task.CompletedBy, task.Comment);
        }

        Workflow.Logger.LogInformation("Workflow finished.");
    }

    private async Task AssessAsync(string? comment, Dictionary<string, string> details)
    {
        try
        {
            var assessment = await AutomatedStepAsync(new LlmRequestStep
            {
                Title = "Assess the request",
                Description = "The LLM checks the approval details and decides whether a manual review is needed.",
                SystemPrompt = "You review approved requests. Select Approved when the budget and the deadline look " +
                               "realistic and consistent, otherwise select Escalated. Give a short reason.",
                Prompt = $"Approval comment: {comment ?? "-"}\n" +
                         string.Join("\n", details.Select(x => $"{x.Key}: {x.Value}")),
                Outcomes = [Approved, Escalated]
            });
            Workflow.Logger.LogInformation("LLM selected {Outcome}: {Reason}", assessment.Outcome, assessment.Content);

            if (assessment.Outcome != Escalated) return;

            var review = await UserStepAsync(new UserTaskStep
            {
                Title = "Review escalated request",
                Description = assessment.Content,
                Outcomes = [Approved, Rejected]
            });
            Workflow.Logger.LogInformation("REVIEWED by {User}: {Result}", review.CompletedBy, review.Result);
        }
        catch (ActivityFailureException e)
        {
            Workflow.Logger.LogWarning("LLM assessment failed: {Error}", e.InnerException?.Message ?? e.Message);
        }
    }
}
