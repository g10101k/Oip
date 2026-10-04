namespace Oip.Hitl.Data.Entities;

/// <summary>
/// Agent exposed to chat UIs as a model: a system prompt, the LLM provider that answers and the skills it may load.
/// </summary>
public class AgentEntity
{
    /// <summary>
    /// Primary key.
    /// </summary>
    public int AgentId { get; set; }

    /// <summary>
    /// Model id of the agent in the OpenAI-compatible API, e.g. <c>support-agent</c>.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name shown in the UI.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description for administrators.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// System prompt of the agent; the list of its skills is added to it.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Provider that answers; <c>null</c> for the default provider.
    /// </summary>
    public int? LlmProviderId { get; set; }

    /// <summary>
    /// Provider that answers.
    /// </summary>
    public LlmProviderEntity? LlmProvider { get; set; }

    /// <summary>
    /// Whether the agent is offered to chat UIs.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Skills the agent may load.
    /// </summary>
    public List<AgentSkillEntity> Skills { get; set; } = [];
}
