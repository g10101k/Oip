using Oip.Base.Extensions;

namespace Oip.Rtds.RandomInterface.Services;

/// <summary>
/// A hosted service that periodically collects and evaluates tags using a specified interval
/// </summary>
public class TagWorkerHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<TagWorkerHostedService> logger,
    TimeSpan? interval = null)
    : BackgroundService
{
    private readonly TimeSpan _interval = interval ?? TimeSpan.FromSeconds(5); // default interval is 5 seconds

    /// <summary>
    /// Executes the background service to collect and evaluate tags periodically.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token that allows stopping the service.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await scopeFactory.ExecuteAsync<TagWorkerService>(x => x.CollectAndEvaluate());
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing tags");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}