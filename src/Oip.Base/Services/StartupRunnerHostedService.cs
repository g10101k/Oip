using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oip.Base.Runtime;

namespace Oip.Base.Services;

public class StartupRunnerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    public StartupRunnerHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var startupRunner = scope.ServiceProvider.GetRequiredService<IStartupRunner>();
        await startupRunner.StartupAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}