using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Oip.Base.Discovery;

/// <summary>
/// Hosted service responsible for starting and stopping service discovery components.
/// </summary>
/// <param name="broadcaster">The service broadcaster.</param>
/// <param name="listener">The service listener.</param>
/// <param name="healthChecker">The service health checker.</param>
/// <param name="logger">The logger.</param>
public sealed class ServiceDiscoveryHostedService(
    IServiceBroadcaster broadcaster,
    IServiceListener listener,
    IServiceHealthChecker healthChecker,
    ILogger<ServiceDiscoveryHostedService> logger)
    : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting service discovery components...");

        listener.StartListeningAsync();
        broadcaster.StartBroadcastingAsync();
        healthChecker.StartHealthCheckingAsync();

        logger.LogInformation("Service discovery components started");
    }


    /// <summary>
    /// Asynchronously stops the service discovery components.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token.</param>
    /// <return>A task that represents the asynchronous operation.</return>
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Stopping service discovery components...");

        await broadcaster.BroadcastUnregistrationAsync();
        await broadcaster.StopBroadcastingAsync();
        await listener.StopListeningAsync();
        await healthChecker.StopHealthCheckingAsync();

        logger.LogInformation("Service discovery components stopped");
    }
}