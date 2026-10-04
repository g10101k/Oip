using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Oip.Hitl.Base.Workflows;

namespace Oip.Hitl.Base.Controllers.Api;

/// <summary>
/// Pending step of a Temporal workflow waiting for a user.
/// </summary>
public class UserStepDto
{
    /// <summary>
    /// Step id, unique within its workflow.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Id of the workflow waiting for the step.
    /// </summary>
    public required string WorkflowInstanceId { get; set; }

    /// <summary>
    /// Angular route of the page that renders the step, relative to the application root.
    /// </summary>
    public required string Route { get; set; }

    /// <summary>
    /// Absolute link to the page that renders the step.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// Step title.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Step description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Page-specific data defined by the workflow, as JSON.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// When the step was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Step status.
    /// </summary>
    public UserStepStatus Status { get; set; }

    /// <summary>
    /// Temporal status of the workflow (Running / Completed / Terminated / ...).
    /// </summary>
    public required string WorkflowStatus { get; set; }

    /// <summary>
    /// When the step was completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Name of the user who completed the step.
    /// </summary>
    public string? CompletedBy { get; set; }

    /// <summary>
    /// Comment left when the step was completed.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Step result as JSON; its shape is defined by the workflow that created the step.
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// Error of a failed automated step.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Files attached to the step.
    /// </summary>
    public List<WorkflowAttachment> Attachments { get; set; } = [];
}

/// <summary>
/// Status of a workflow user step.
/// </summary>
public enum UserStepStatus
{
    /// <summary>
    /// Waits for a user.
    /// </summary>
    Pending,

    /// <summary>
    /// Completed by a user.
    /// </summary>
    Completed,

    /// <summary>
    /// Not completed and will not be: the workflow was closed while waiting for it.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Automated step performed by the workflow right now.
    /// </summary>
    Running,

    /// <summary>
    /// Automated step that ended with an error.
    /// </summary>
    Failed
}

/// <summary>
/// Data sent by the page when the user completes a step.
/// </summary>
public class CompleteUserStepRequest
{
    /// <summary>
    /// Step result as JSON; its shape is defined by the workflow that created the step.
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// Optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// File uploaded to complete a step.
/// </summary>
public class UploadStepFileRequest
{
    /// <summary>
    /// Uploaded file.
    /// </summary>
    [Required]
    public IFormFile File { get; set; } = null!;
}

/// <summary>
/// Result of completing a step.
/// </summary>
public class CompleteUserStepResponse
{
    /// <summary>
    /// Id of the resumed workflow.
    /// </summary>
    public required string WorkflowInstanceId { get; set; }

    /// <summary>
    /// Temporal workflow status after the step was completed (Running / Completed / ...).
    /// </summary>
    public required string Status { get; set; }
}
