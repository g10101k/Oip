using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oip.Hil.Base.Settings;
using Temporalio.Client;
using Temporalio.Worker;

namespace Oip.Hil.Base.Workflows;

/// <summary>
/// Runs the Temporal worker for the workflows in <see cref="OipWorkflowOptions"/>. When the server is unreachable
/// the worker is restarted after a delay instead of stopping the application, so the rest of the app works
/// without Temporal.
/// </summary>
public class TemporalWorkerService(
    ITemporalClient client,
    TemporalSettings settings,
    OipWorkflowOptions workflowOptions,
    ILoggerFactory loggerFactory,
    ILogger<TemporalWorkerService> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await client.Connection.ConnectAsync();
                using var worker = new TemporalWorker(client, CreateWorkerOptions());
                logger.LogInformation("Temporal worker started on task queue {TaskQueue} at {Address}",
                    settings.TaskQueue, settings.Address);
                await worker.ExecuteAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Temporal worker is not running, retrying in {Delay}", RetryDelay);
                try
                {
                    await Task.Delay(RetryDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    private TemporalWorkerOptions CreateWorkerOptions()
    {
        var options = new TemporalWorkerOptions(settings.TaskQueue) { LoggerFactory = loggerFactory };
        foreach (var type in workflowOptions.Workflows)
            options.AddWorkflow(type);
        return options;
    }
}
