using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Oip.Base.Runtime;

/// <summary>
/// Runner for startup tasks.
/// </summary>
public class StartupRunner(
    IEnumerable<IStartupTask> startupTasks,
    IServiceScopeFactory scopeFactory,
    ILogger<StartupRunner> logger) : IStartupRunner
{
    /// <summary>
    /// Executes startup tasks in order.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StartupAsync(CancellationToken cancellationToken = default)
    {
        var startupTaskTypes = startupTasks.OrderBy(x => x.Order).Select(x => x.GetType()).ToList();

        foreach (var startupTaskType in startupTaskTypes)
        {
            using var scope = scopeFactory.CreateScope();
            var startupTask = (IStartupTask)scope.ServiceProvider.GetRequiredService(startupTaskType);
            logger.LogInformation("Running startup task {StartupTaskName}", startupTaskType.Name);
            await startupTask.ExecuteAsync(cancellationToken);
        }
    }
}