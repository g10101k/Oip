using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Oip.Base.Runtime;

public class StartupRunner : IStartupRunner
{
    private readonly ILogger<StartupRunner> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICollection<Type> _startupTaskTypes;

    public StartupRunner(IEnumerable<IStartupTask> startupTasks, IServiceScopeFactory scopeFactory,
        ILogger<StartupRunner> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _startupTaskTypes = startupTasks.OrderBy(x => x.Order).Select(x => x.GetType()).ToList();
    }

    public async Task StartupAsync(CancellationToken cancellationToken = default)
    {
        foreach (var startupTaskType in _startupTaskTypes)
        {
            using var scope = _scopeFactory.CreateScope();
            var startupTask = (IStartupTask)scope.ServiceProvider.GetRequiredService(startupTaskType);
            _logger.LogInformation("Running startup task {StartupTaskName}", startupTaskType.Name);
            await startupTask.ExecuteAsync(cancellationToken);
        }
    }
}