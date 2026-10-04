namespace Oip.Hitl.AiFunctions;

/// <summary>
/// Marks a Temporal activity as a tool skills can give agents. The method takes no parameters or one class of the
/// arguments, whose JSON schema is shown to the model: name its properties with <c>JsonPropertyName</c> and describe
/// them with <c>Description</c>, as the method itself. It returns a string or an object serialized to JSON.
/// </summary>
/// <param name="name">Name of the tool, e.g. <c>get_current_time</c>.</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AgentToolAttribute(string name) : Attribute
{
    /// <summary>
    /// Name of the tool.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Timeout of an attempt in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 60;

    /// <summary>
    /// Attempts before the failure is returned to the model.
    /// </summary>
    public int MaxAttempts { get; init; } = 3;
}
