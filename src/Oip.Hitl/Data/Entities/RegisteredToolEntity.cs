namespace Oip.Hitl.Data.Entities;

/// <summary>
/// Tool registered by a skill worker: an activity of the worker the model may call, on the task queue of the worker.
/// The worker replaces its tools when it starts.
/// </summary>
public class RegisteredToolEntity
{
    /// <summary>
    /// Name of the tool, the primary key.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// What the tool does.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// JSON schema of the arguments object.
    /// </summary>
    public string ParametersSchema { get; set; } = string.Empty;

    /// <summary>
    /// Temporal name of the activity.
    /// </summary>
    public string ActivityName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the activity takes the arguments object as its parameter.
    /// </summary>
    public bool HasArguments { get; set; }

    /// <summary>
    /// Task queue the worker of the tool polls.
    /// </summary>
    public string TaskQueue { get; set; } = string.Empty;

    /// <summary>
    /// Timeout of an attempt in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// Attempts before the failure is returned to the model.
    /// </summary>
    public int MaxAttempts { get; set; }

    /// <summary>
    /// Whether the user allows each call before it is made.
    /// </summary>
    public bool RequiresApproval { get; set; }

    /// <summary>
    /// When the worker registered the tool (UTC).
    /// </summary>
    public DateTime RegisteredAt { get; set; }
}
