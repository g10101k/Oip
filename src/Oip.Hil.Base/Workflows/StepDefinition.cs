namespace Oip.Hil.Base.Workflows;

/// <summary>
/// Common part of the workflow steps shown in the task list: the Angular page that renders the step and the data
/// passed to the page.
/// </summary>
public abstract class StepDefinition
{
    /// <summary>
    /// Angular route of the page that renders the step, relative to the application root.
    /// </summary>
    public abstract string Route { get; }

    /// <summary>
    /// Title shown to the user.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Description shown to the user.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Page-specific data, passed to the page as camelCase JSON.
    /// </summary>
    public virtual object? Data => null;
}
