namespace Oip.Hitl.Data.Entities;

/// <summary>
/// Tool a skill gives the model, by its name in the tool catalog.
/// </summary>
public class SkillToolEntity
{
    /// <summary>
    /// Skill.
    /// </summary>
    public int SkillId { get; set; }

    /// <summary>
    /// Name of the tool, e.g. <c>get_current_time</c>.
    /// </summary>
    public string ToolName { get; set; } = string.Empty;

    /// <summary>
    /// Skill.
    /// </summary>
    public SkillEntity Skill { get; set; } = null!;
}
