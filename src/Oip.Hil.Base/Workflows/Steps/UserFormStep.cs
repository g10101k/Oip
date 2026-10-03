namespace Oip.Hil.Base.Workflows.Steps;

/// <summary>
/// Form completed on the <c>workflow-form</c> page; the result contains a non-empty value for every field.
/// </summary>
public class UserFormStep : UserStepDefinition<Dictionary<string, string>>
{
    /// <inheritdoc />
    public override string Route => "workflow-form";

    /// <summary>
    /// Names of the required fields.
    /// </summary>
    public required IReadOnlyList<string> Fields
    {
        get;
        init => field = value.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
    }

    /// <inheritdoc />
    public override object Data => new { Fields };

    /// <inheritdoc />
    public override string? Validate(Dictionary<string, string> result)
    {
        var missing = Fields
            .Where(name => !result.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
            .ToList();
        return missing.Count > 0 ? $"Required fields are empty: {string.Join(", ", missing)}" : null;
    }

    /// <inheritdoc />
    public override Dictionary<string, string> Normalize(Dictionary<string, string> result) =>
        Fields.ToDictionary(name => name, name => result[name].Trim());
}
