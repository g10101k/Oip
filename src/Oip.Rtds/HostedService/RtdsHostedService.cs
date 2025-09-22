using Oip.Base.Extensions;
using Oip.Rtds.Grpc;
using Oip.Rts.Services;

namespace Oip.Rts.HostedService;

public class RtdsHostedService : BackgroundService
{
    private readonly ILogger<RtdsHostedService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public RtdsHostedService(IServiceScopeFactory scopeFactory, ILogger<RtdsHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }


    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service running.");

        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service is working.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await _scopeFactory.ExecuteAsync<OipRtdsService>(async service =>
            {
                _logger.LogInformation(
                    "Publishing events...");

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
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}