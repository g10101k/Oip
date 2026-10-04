namespace Oip.Hitl.Data.Entities;

/// <summary>
/// Skill an agent may load.
/// </summary>
public class AgentSkillEntity
{
    /// <summary>
    /// Agent.
    /// </summary>
    public int AgentId { get; set; }

    /// <summary>
    /// Skill.
    /// </summary>
    public int SkillId { get; set; }

    /// <summary>
    /// Agent.
    /// </summary>
    public AgentEntity Agent { get; set; } = null!;

    /// <summary>
    /// Skill.
    /// </summary>
    public SkillEntity Skill { get; set; } = null!;
}
