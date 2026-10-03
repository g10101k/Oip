namespace Oip.Hil.Base.Workflows.Steps;

/// <summary>
/// Task completed on the <c>workflow-task</c> page. When <see cref="Outcomes"/> are set, the result is one of them;
/// otherwise it is a free-text result.
/// </summary>
public class UserTaskStep : UserStepDefinition<string>
{
    /// <inheritdoc />
    public override string Route => "workflow-task";

    /// <summary>
    /// Outcomes the user chooses from.
    /// </summary>
    public IReadOnlyList<string> Outcomes
    {
        get;
        init => field = value.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
    } = [];

    /// <inheritdoc />
    public override object Data => new { Outcomes };

    /// <inheritdoc />
    public override string? Validate(string result) =>
        Outcomes.Count > 0 && !Outcomes.Contains(result)
            ? $"Result must be one of: {string.Join(", ", Outcomes)}"
            : null;

    /// <inheritdoc />
    public override string Normalize(string result) => result.Trim();
}
