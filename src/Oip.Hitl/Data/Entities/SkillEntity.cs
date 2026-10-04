namespace Oip.Hitl.Data.Entities;

/// <summary>
/// Skill of agents: instructions and tools for a kind of task. The model sees only <see cref="Description"/> until
/// it loads the skill; then it gets <see cref="Instructions"/> and the tools.
/// </summary>
public class SkillEntity
{
    /// <summary>
    /// Primary key.
    /// </summary>
    public int SkillId { get; set; }

    /// <summary>
    /// Name the model loads the skill by, e.g. <c>datetime</c>.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// When to use the skill; always shown to the model.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// How to do the task; given to the model when it loads the skill.
    /// </summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>
    /// Whether agents may load the skill.
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
    /// Tools the skill gives the model.
    /// </summary>
    public List<SkillToolEntity> Tools { get; set; } = [];

    /// <summary>
    /// Agents that may load the skill.
    /// </summary>
    public List<AgentSkillEntity> Agents { get; set; } = [];
}
