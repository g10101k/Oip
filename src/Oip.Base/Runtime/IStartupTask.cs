namespace Oip.Base.Runtime;

/// <summary>
/// Defines a task to be executed during application startup.
/// </summary>
public interface IStartupTask
{
    /// <summary>
    /// Defines the order in which the startup task should be executed.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Executes the startup task.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}