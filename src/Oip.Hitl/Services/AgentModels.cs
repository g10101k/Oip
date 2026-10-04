using System.Text.Json;

namespace Oip.Hitl.Services;

/// <summary>
/// Agent as an agent run sees it: taken when the run starts, so later changes do not affect it.
/// </summary>
/// <param name="AgentId">Agent.</param>
/// <param name="Code">Model id of the agent.</param>
/// <param name="ProviderId">Provider that answers; <c>null</c> for the default provider.</param>
/// <param name="SystemPrompt">System prompt of the agent.</param>
/// <param name="Skills">Enabled skills the agent may load.</param>
public record AgentSnapshot(
    int AgentId,
    string Code,
    int? ProviderId,
    string? SystemPrompt,
    IReadOnlyList<AgentSkillSummary> Skills);

/// <summary>
/// Skill as the model sees it before loading it.
/// </summary>
/// <param name="Code">Name the model loads the skill by.</param>
/// <param name="Description">When to use the skill.</param>
public record AgentSkillSummary(string Code, string Description);

/// <summary>
/// Request to load a skill of an agent.
/// </summary>
/// <param name="AgentId">Agent that loads the skill.</param>
/// <param name="Code">Skill to load.</param>
public record LoadSkillRequest(int AgentId, string Code);

/// <summary>
/// Loaded skill: what the model gets after loading it.
/// </summary>
/// <param name="Code">Skill.</param>
/// <param name="Instructions">How to do the task.</param>
/// <param name="Tools">Tools the skill gives the model.</param>
public record LoadedSkill(string Code, string Instructions, IReadOnlyList<AgentToolDefinition> Tools);

/// <summary>
/// Tool of the tool catalog: a Temporal activity the model may call.
/// </summary>
/// <param name="Name">Name of the tool, e.g. <c>get_current_time</c>.</param>
/// <param name="Description">What the tool does.</param>
/// <param name="ParametersSchema">JSON schema of the arguments object.</param>
/// <param name="ActivityName">Temporal name of the activity.</param>
/// <param name="HasArguments">Whether the activity takes the arguments object as its parameter.</param>
/// <param name="TaskQueue">Task queue of the worker of the activity; <c>null</c> for the queue of the workflow.</param>
/// <param name="TimeoutSeconds">Timeout of an attempt.</param>
/// <param name="MaxAttempts">Attempts before the failure is returned to the model.</param>
public record AgentToolDefinition(
    string Name,
    string Description,
    JsonElement ParametersSchema,
    string ActivityName,
    bool HasArguments,
    string? TaskQueue,
    int TimeoutSeconds,
    int MaxAttempts);
