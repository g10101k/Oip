using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Oip.Base.Services;

/// <summary>
/// Provides a base implementation for executing periodic background work using a specified worker service at configurable intervals.
/// Use it if interval great 15 second
/// </summary>
public class PeriodicBackgroundService<TScopeServiceWorker>(
    IServiceScopeFactory scopeFactory,
    ILogger<PeriodicBackgroundService<TScopeServiceWorker>> logger)
    : BackgroundService where TScopeServiceWorker : class, IPeriodicalService
{
    /// <summary>
    /// Executes the periodic background work asynchronously using the configured worker service until the cancellation token is triggered
    /// </summary>
    /// <param name="stoppingToken">The token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Service started.");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                var worker = GetWorker(scope);
                if (worker is null || worker.Interval <= 0)
                {
                    if (worker is null)
                        logger.LogWarning("Worker is null. Waiting 60 seconds.");
                    else if (worker.Interval < 0)
                        logger.LogInformation("Interval <= 0, worker not accepted. Waiting 60 seconds.");
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                    continue;
                }

                try
                {
                    logger.LogDebug("Work started at: {Time}, interval: {Interval}", DateTimeOffset.UtcNow,
                        worker.Interval);

                    await worker.ExecuteAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "PeriodicService failed.");
                }

                await Task.Delay(TimeSpan.FromSeconds(worker.Interval), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Service worker canceled.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "PeriodicService failed.");
        }
    }

    private TScopeServiceWorker? GetWorker(IServiceScope scope)
    {
        try
        {
            return scope.ServiceProvider.GetRequiredService<TScopeServiceWorker>();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Cannot create instance of {TWorker}", typeof(TScopeServiceWorker));
        }

        return null;
    }
}

/// <summary>
/// Defines a service that executes periodic background work at a specified interval
/// </summary>
public interface IPeriodicalService
{
    /// <summary>
    /// Gets the interval in seconds at which the periodic background work should be executed
    /// Must be a positive value. Negative or zero values will result in the service being skipped.
    /// </summary>
    int Interval { get; }

    /// <summary>
    /// Executes the periodic background work asynchronously until the cancellation token is triggered
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task ExecuteAsync(CancellationToken cancellationToken);
}