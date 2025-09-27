using Grpc.Core;
using Grpc.Net.Client;
using Oip.Base.Extensions;
using Oip.Rtds.Grpc;
using Oip.Rtds.Random.Settings;

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