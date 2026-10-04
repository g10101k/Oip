using System.Text.Json;
using Temporalio.Exceptions;
using Temporalio.Workflows;

namespace Oip.Hitl.Base.Workflows;

/// <summary>
/// Base class for Temporal workflows that wait for users. <see cref="UserStepAsync{TResult}"/> registers a
/// <see cref="UserStep"/> rendered by an Angular page and completed through the <see cref="CompleteStepUpdate"/>
/// update. All steps of the workflow, pending and completed, are kept in the <see cref="StepsMemo"/> memo, so they
/// are read with the workflow list, also after the workflow is closed. <see cref="AutomatedStepAsync{TResult}"/>
/// records a step the workflow performs itself (for example an activity call), so it is shown in the same list.
/// Kinds of steps and their pages are defined by the application as <see cref="UserStepDefinition{TResult}"/> and
/// <see cref="AutomatedStepDefinition{TResult}"/>.
/// </summary>
public abstract class UserWorkflowBase
{
    /// <summary>
    /// Memo key with the list of all <see cref="UserStep"/>s of the workflow.
    /// </summary>
    public const string StepsMemo = "UserSteps";

    /// <summary>
    /// Name of the update that completes a step.
    /// </summary>
    public const string CompleteStepUpdate = "CompleteUserStep";

    /// <summary>
    /// Error type of the update failure when the step does not exist or is already completed.
    /// </summary>
    public const string StepNotFoundError = "UserStepNotFound";

    /// <summary>
    /// Error type of the update failure when the step result is invalid.
    /// </summary>
    public const string StepValidationError = "UserStepValidation";

    private static readonly JsonSerializerOptions JsonOptions = JsonSerializerOptions.Web;

    private readonly List<UserStep> steps = [];
    private readonly Dictionary<string, PendingStep> pendingSteps = new();
    private readonly Dictionary<string, UserStepCompletion> completions = new();

    /// <summary>
    /// Rejects the completion of an unknown step or with an invalid result before it is written to the history.
    /// </summary>
    [WorkflowUpdateValidator(nameof(CompleteStepAsync))]
    public void ValidateCompleteStep(UserStepCompletion completion)
    {
        if (!pendingSteps.TryGetValue(completion.StepId, out var pending))
            throw new ApplicationFailureException($"Step {completion.StepId} not found or already completed",
                StepNotFoundError);

        var error = pending.Validate(completion.Result);
        if (error is not null)
            throw new ApplicationFailureException(error, StepValidationError);
    }

    /// <summary>
    /// Completes a pending step and resumes the code waiting for it.
    /// </summary>
    [WorkflowUpdate(CompleteStepUpdate)]
    public Task CompleteStepAsync(UserStepCompletion completion)
    {
        pendingSteps.Remove(completion.StepId);
        completions[completion.StepId] = completion;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Waits until a user completes the step on the page at <see cref="StepDefinition.Route"/>.
    /// </summary>
    /// <param name="definition">Step to show to the user.</param>
    protected async Task<UserStepResult<TResult>> UserStepAsync<TResult>(UserStepDefinition<TResult> definition)
    {
        var step = CreateStep(definition, automated: false);
        pendingSteps[step.Id] = new PendingStep(step, raw => TryRead<TResult>(raw, out var result)
            ? definition.Validate(result)
            : "Result is missing or has a wrong format");
        steps.Add(step);
        UpsertStepsMemo();

        await Workflow.WaitConditionAsync(() => completions.ContainsKey(step.Id));

        completions.Remove(step.Id, out var completion);
        TryRead<TResult>(completion!.Result, out var value);
        var result = definition.Normalize(value);

        step.CompletedAt = Workflow.UtcNow;
        step.CompletedBy = completion.CompletedBy;
        step.Comment = completion.Comment;
        step.Result = JsonSerializer.SerializeToElement(result, JsonOptions);
        step.Attachments = definition.GetAttachments(result).ToList();
        UpsertStepsMemo();

        return new UserStepResult<TResult>(result, completion.Comment, completion.CompletedBy);
    }

    /// <summary>
    /// Performs a step without a user and records it in the step list, so its progress and result are shown on
    /// the page at <see cref="StepDefinition.Route"/>. A failure is recorded as the step error and rethrown.
    /// </summary>
    /// <param name="definition">Step to perform.</param>
    protected async Task<TResult> AutomatedStepAsync<TResult>(AutomatedStepDefinition<TResult> definition)
    {
        var step = CreateStep(definition, automated: true);
        steps.Add(step);
        UpsertStepsMemo();

        TResult result;
        try
        {
            result = await definition.RunAsync(new AutomatedStepContext(Workflow.Info.WorkflowId, step.Id));
        }
        catch (FailureException e)
        {
            step.CompletedAt = Workflow.UtcNow;
            step.Error = e.InnerException?.Message ?? e.Message;
            UpsertStepsMemo();
            throw;
        }

        step.CompletedAt = Workflow.UtcNow;
        step.CompletedBy = definition.GetPerformer(result);
        step.Result = JsonSerializer.SerializeToElement(result, JsonOptions);
        step.Attachments = definition.GetAttachments(result).ToList();
        UpsertStepsMemo();

        return result;
    }

    private static UserStep CreateStep(StepDefinition definition, bool automated)
    {
        var data = definition.Data;
        return new UserStep
        {
            Id = Workflow.NewGuid().ToString("N"),
            Route = definition.Route.Trim('/'),
            Title = definition.Title,
            Description = definition.Description,
            Data = data is null ? null : JsonSerializer.SerializeToElement(data, JsonOptions),
            CreatedAt = Workflow.UtcNow,
            Automated = automated
        };
    }

    private void UpsertStepsMemo() => Workflow.UpsertMemo(MemoUpdate.ValueSet(StepsMemo, steps));

    private static bool TryRead<T>(JsonElement? raw, out T value)
    {
        value = default!;
        if (raw is not { ValueKind: not JsonValueKind.Null } element) return false;
        try
        {
            value = element.Deserialize<T>(JsonOptions)!;
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private sealed record PendingStep(UserStep Step, Func<JsonElement?, string?> Validate);
}
