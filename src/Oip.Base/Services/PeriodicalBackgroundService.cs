using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Oip.Base.Services;

public class PeriodicalBackgroundService<TWorker>(IServiceScopeFactory scopeFactory, ILogger<TWorker> logger)
    : BackgroundService where TWorker : class, IPeriodicalService
{
    public const string WorkerIsNullWaitingSeconds = "Worker is null. Waiting 60 seconds.";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var worker = GetWorker(scope);
            if (worker is null || worker.Interval < 0)
            {
                if (worker is null)
                    logger.LogInformation(WorkerIsNullWaitingSeconds);
                else if (worker.Interval < 0)
                    logger.LogWarning("Interval < 0, worker not accepted. Waiting 60 seconds.");
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                continue;
            }

            try
            {
                logger.LogInformation("Work started at: {Time}, interval: {Interval}", DateTimeOffset.Now,
                    worker.Interval);

                await worker.ExecuteAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogError("Сервис остановлен");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при выполнении");
            }

            await Task.Delay(TimeSpan.FromSeconds(worker.Interval), stoppingToken);
        }

        logger.LogInformation("Service stopped.");
    }

    private TWorker? GetWorker(IServiceScope scope)
    {
        try
        {
            return ActivatorUtilities.GetServiceOrCreateInstance<TWorker>(scope.ServiceProvider);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Cannot create instance of {TWorker}", typeof(TWorker));
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
    /// </summary>
    int Interval { get; }

    /// <summary>
    /// Executes the periodic background work asynchronously until the cancellation token is triggered
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task ExecuteAsync(CancellationToken cancellationToken);
}