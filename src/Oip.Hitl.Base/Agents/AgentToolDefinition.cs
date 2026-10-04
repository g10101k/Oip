using System.Text.Json;

namespace Oip.Hitl.Base.Agents;

/// <summary>
/// Tool of the tool catalog: a Temporal activity the model may call, of Oip.Hitl or of a skill worker.
/// </summary>
/// <param name="Name">Name of the tool, e.g. <c>get_current_time</c>.</param>
/// <param name="Description">What the tool does.</param>
/// <param name="ParametersSchema">JSON schema of the arguments object.</param>
/// <param name="ActivityName">Temporal name of the activity.</param>
/// <param name="HasArguments">Whether the activity takes the arguments object as its parameter.</param>
/// <param name="TaskQueue">Task queue of the worker of the activity; <c>null</c> for the queue of the workflow.</param>
/// <param name="TimeoutSeconds">Timeout of an attempt.</param>
/// <param name="MaxAttempts">Attempts before the failure is returned to the model.</param>
/// <param name="RequiresApproval">Whether the user allows each call before it is made.</param>
public record AgentToolDefinition(
    string Name,
    string Description,
    JsonElement ParametersSchema,
    string ActivityName,
    bool HasArguments,
    string? TaskQueue,
    int TimeoutSeconds,
    int MaxAttempts,
    bool RequiresApproval = false);
