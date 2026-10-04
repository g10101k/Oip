namespace Oip.Hitl.Controllers.Api;

/// <summary>
/// Settings for the Agent module instance.
/// </summary>
public class AgentModuleSettings
{
    /// <summary>
    /// Show disabled agents and skills in the lists.
    /// </summary>
    public bool ShowDisabled { get; set; } = true;
}

/// <summary>
/// Agent as exposed to the client.
/// </summary>
/// <param name="Id">Primary key.</param>
/// <param name="Code">Model id of the agent in the OpenAI-compatible API.</param>
/// <param name="Name">Display name.</param>
/// <param name="Description">Description for administrators.</param>
/// <param name="SystemPrompt">System prompt of the agent.</param>
/// <param name="LlmProviderId">Provider that answers; null for the default provider.</param>
/// <param name="IsEnabled">Whether the agent is offered to chat UIs.</param>
/// <param name="SkillIds">Skills the agent may load.</param>
/// <param name="CreatedAt">Creation timestamp (UTC).</param>
/// <param name="UpdatedAt">Last update timestamp (UTC).</param>
public record AgentDto(
    int Id,
    string Code,
    string Name,
    string? Description,
    string? SystemPrompt,
    int? LlmProviderId,
    bool IsEnabled,
    List<int> SkillIds,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Create or update request for an agent.
/// </summary>
/// <param name="Code">Model id (unique): lowercase letters, digits, dots, dashes and underscores.</param>
/// <param name="Name">Display name.</param>
/// <param name="Description">Description for administrators.</param>
/// <param name="SystemPrompt">System prompt of the agent.</param>
/// <param name="LlmProviderId">Provider that answers; null for the default provider.</param>
/// <param name="IsEnabled">Whether the agent is offered to chat UIs.</param>
/// <param name="SkillIds">Skills the agent may load.</param>
public record SaveAgentRequest(
    string Code,
    string Name,
    string? Description = null,
    string? SystemPrompt = null,
    int? LlmProviderId = null,
    bool IsEnabled = true,
    List<int>? SkillIds = null);

/// <summary>
/// Skill as exposed to the client.
/// </summary>
/// <param name="Id">Primary key.</param>
/// <param name="Code">Name the model loads the skill by.</param>
/// <param name="Description">When to use the skill; always shown to the model.</param>
/// <param name="Instructions">How to do the task; given to the model when it loads the skill.</param>
/// <param name="IsEnabled">Whether agents may load the skill.</param>
/// <param name="Tools">Names of the tools the skill gives the model.</param>
/// <param name="CreatedAt">Creation timestamp (UTC).</param>
/// <param name="UpdatedAt">Last update timestamp (UTC).</param>
public record SkillDto(
    int Id,
    string Code,
    string Description,
    string Instructions,
    bool IsEnabled,
    List<string> Tools,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Create or update request for a skill.
/// </summary>
/// <param name="Code">Name (unique) the model loads the skill by: lowercase letters, digits, dots, dashes and underscores.</param>
/// <param name="Description">When to use the skill; always shown to the model.</param>
/// <param name="Instructions">How to do the task; given to the model when it loads the skill.</param>
/// <param name="IsEnabled">Whether agents may load the skill.</param>
/// <param name="Tools">Names of the tools from the tool catalog the skill gives the model.</param>
public record SaveSkillRequest(
    string Code,
    string Description,
    string Instructions,
    bool IsEnabled = true,
    List<string>? Tools = null);

/// <summary>
/// Tool of the tool catalog.
/// </summary>
/// <param name="Name">Name of the tool.</param>
/// <param name="Description">What the tool does.</param>
/// <param name="ParametersSchema">JSON schema of the arguments.</param>
public record AgentToolDto(string Name, string Description, string ParametersSchema);
