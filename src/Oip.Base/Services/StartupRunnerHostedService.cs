using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oip.Base.Runtime;

namespace Oip.Base.Services;

/// <summary>
/// Hosted service that initiates the application startup pipeline.
/// When the host starts, it resolves an <see cref="IStartupRunner"/> from the
/// DI container and invokes its <see cref="IStartupRunner.StartupAsync"/> method.
/// </summary>
public class StartupRunnerHostedService(IServiceProvider serviceProvider, ILogger<StartupRunnerHostedService> logger)
    : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var startupRunner = scope.ServiceProvider.GetRequiredService<IStartupRunner>();
            await startupRunner.StartupAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Cancelling startup runner.");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}