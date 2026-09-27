namespace Oip.Hil.Base.Workflows;

/// <summary>
/// Describes a kind of user step: the Angular page that renders it, the data passed to the page and the check of
/// the result. Applications define their own steps next to their pages and pass them to
/// <see cref="UserWorkflowBase.UserStepAsync{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">Type of the result the page sends back.</typeparam>
public abstract class UserStepDefinition<TResult>
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

    /// <summary>
    /// Returns an error for an invalid result, or <c>null</c> when it is valid.
    /// </summary>
    public virtual string? Validate(TResult result) => null;

    /// <summary>
    /// Converts a valid result before it is returned to the workflow code.
    /// </summary>
    public virtual TResult Normalize(TResult result) => result;
}
