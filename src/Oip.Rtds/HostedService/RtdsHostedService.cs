using Oip.Base.Extensions;
using Oip.Rtds.Grpc;
using RtdsService = Oip.Rts.Services.RtdsService;

namespace Oip.Rts.HostedService;

public class RtdsHostedService(IServiceScopeFactory scopeFactory, ILogger<RtdsHostedService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Consume Scoped Service Hosted Service running.");

        logger.LogInformation("Consume Scoped Service Hosted Service is working.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await scopeFactory.ExecuteAsync<RtdsService>(async service =>
            {
                logger.LogInformation("Publishing events...");

                var testRequest = new PublishRequest
                {
                    EventType = "eventType",
                    Payload = $"Test event payload: {DateTime.UtcNow}"
                };
                await service.Publish(testRequest, null!);
            });

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
    
    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Consume Scoped Service Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}