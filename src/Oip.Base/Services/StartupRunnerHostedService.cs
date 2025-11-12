using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oip.Base.Runtime;

namespace Oip.Base.Services;

/// <summary>
/// Hosted service that initiates the application startup pipeline.
/// When the host starts, it resolves an <see cref="IStartupRunner"/> from the
/// DI container and invokes its <see cref="IStartupRunner.StartupAsync"/> method.
/// </summary>
public class StartupRunnerHostedService(IServiceProvider serviceProvider) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var startupRunner = scope.ServiceProvider.GetRequiredService<IStartupRunner>();
        await startupRunner.StartupAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}