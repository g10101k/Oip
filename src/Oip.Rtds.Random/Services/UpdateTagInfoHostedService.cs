using Oip.Base.Extensions;

namespace Oip.Rtds.Random.Services;

/// <summary>
/// This namespace contains services related to generating random data for RTDS.
/// It provides functionalities for creating various types of random values,
/// potentially used for simulation or testing purposes within the RTDS environment.
/// </summary>
/// <param name="logger"></param>
/// <param name="scopeFactory"></param>
public class UpdateTagInfoHostedService(ILogger<UpdateTagInfoHostedService> logger, IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    /// <summary>
    /// This method is called when the hosted service starts. It's responsible for setting up
    /// any necessary resources or configurations.
    /// </summary>
    /// <param name="stoppingToken">A token that signals when the hosted service should stop.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Your update logic here
                await Task.Delay(1000, stoppingToken); // Example delay
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating tag information.");
            }
        }
    }
}
    