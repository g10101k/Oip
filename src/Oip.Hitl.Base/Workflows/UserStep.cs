using System.Text.Json;

namespace Oip.Hitl.Base.Workflows;

/// <summary>
/// Step a workflow waits for a user to complete on an Angular page.
/// </summary>
public class UserStep
{
    /// <summary>
    /// Step id, unique within its workflow.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Angular route of the page that renders the step, relative to the application root.
    /// </summary>
    public required string Route { get; set; }

    /// <summary>
    /// Title shown to the user.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Description shown to the user.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Page-specific data (for example outcomes or form fields), serialized in camelCase.
    /// </summary>
    public JsonElement? Data { get; set; }

    /// <summary>
    /// When the step was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the step was completed (UTC); <c>null</c> while it is pending.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Name of the user who completed the step.
    /// </summary>
    public string? CompletedBy { get; set; }

    /// <summary>
    /// Comment left when the step was completed.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Step result as returned to the workflow code, serialized in camelCase.
    /// </summary>
    public JsonElement? Result { get; set; }

    /// <summary>
    /// Whether the workflow performs the step itself instead of waiting for a user.
    /// </summary>
    public bool Automated { get; set; }

    /// <summary>
    /// Error of a failed automated step; <c>null</c> when the step did not fail.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Files attached to the step.
    /// </summary>
    public List<WorkflowAttachment> Attachments { get; set; } = [];
}

/// <summary>
/// Data the user sent when completing a <see cref="UserStep"/>.
/// </summary>
public class UserStepCompletion
{
    /// <summary>
    /// Id of the completed step.
    /// </summary>
    public required string StepId { get; set; }

    /// <summary>
    /// Step result; its shape is defined by the workflow that created the step.
    /// </summary>
    public JsonElement? Result { get; set; }

    /// <summary>
    /// Optional comment.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Name of the user who completed the step.
    /// </summary>
    public string? CompletedBy { get; set; }
}

/// <summary>
/// Completed step as seen by the workflow code.
/// </summary>
/// <param name="Result">Typed step result.</param>
/// <param name="Comment">Optional comment.</param>
/// <param name="CompletedBy">Name of the user who completed the step.</param>
public record UserStepResult<TResult>(TResult Result, string? Comment, string? CompletedBy);
