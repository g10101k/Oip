using Oip.Base.Extensions;

namespace Oip.Rtds.Random.Services;

public class RandomInterfaceServices(ILogger<RandomInterfaceServices> logger, IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                scopeFactory.ExecuteScoped<RandomInterfaceScoped>(x=>x.DoWork());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception.");
            }

            await Task.Delay(50000, stoppingToken);
        }
    }
}