using Oip.Base.Extensions;
using Oip.Rtds.Grpc;
using RtdsService = Oip.Rtds.Services.RtdsService;

namespace Oip.Rtds.HostedService;

/// <summary>
/// Background service that periodically publishes test events using <see cref="RtdsService"/>.
/// </summary>
public class RtdsHostedService(IServiceScopeFactory scopeFactory, ILogger<RtdsHostedService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Consume Scoped Service Hosted Service running.");
        
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