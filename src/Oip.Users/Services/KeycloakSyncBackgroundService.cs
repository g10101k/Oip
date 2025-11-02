using Oip.Base.Extensions;
using Oip.Users.Settings;

namespace Oip.Users.Services;

/// <summary>
/// Background service for synchronizing users periodically.
/// </summary>
public class KeycloakSyncBackgroundService(
    ILogger<KeycloakSyncBackgroundService> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly SyncOptions _syncOptions = AppSettings.Instance.SyncOptions;

    /// <summary>
    /// Starts the background synchronization service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token to stop the service.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_syncOptions.IntervalSeconds > 0)
        {
            logger.LogInformation("Consume Scoped Service Hosted Service running.");
            await DoWork(stoppingToken);
        }
        else
        {
            logger.LogInformation("Background synchronization is disabled");
        }
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        logger.LogInformation("Consume Scoped Service Hosted Service is working.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await scopeFactory.ExecuteAsync<UserSyncService>(async service =>
            {
                logger.LogInformation("Starting scheduled synchronization");
                await service.SyncAllUsersAsync();
                logger.LogInformation("Scheduled synchronization completed");
            });

            await Task.Delay(TimeSpan.FromSeconds(_syncOptions.IntervalSeconds), stoppingToken);
        }
    }
}