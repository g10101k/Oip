using System.Text;
using Grpc.Core;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.RandomInterface.Services;

public class Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<RtdsService.RtdsServiceClient>();
                var clientId = $"client_{Guid.NewGuid().ToString()[..8]}";
                var eventTypes = new[] { "system.alert", "user.notification" };

                logger.LogInformation("Connecting as {ClientId}...", clientId);
                logger.LogInformation("Subscribing to: {Join}", string.Join(", ", eventTypes));
                var request = new SubscribeRequest
                {
                    ClientId = clientId,
                    EventTypes = { eventTypes }
                };

                using var call = client.Subscribe(request);
                var responseStream = call.ResponseStream;

                await foreach (var eventMessage in responseStream.ReadAllAsync(cancellationToken: stoppingToken))
                {
                    DisplayEvent(eventMessage);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                logger.LogError(ex, "Subscription was cancelled.");
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
            {
                logger.LogError(ex, "Service is unavailable.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception.");
            }

            await Task.Delay(10000, stoppingToken);
        }
    }

    private void DisplayEvent(EventMessage eventMessage)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("");
        sb.AppendLine("=== NEW EVENT ===");
        sb.AppendLine($"ID: {eventMessage.EventId}");
        sb.AppendLine($"Type: {eventMessage.EventType}");
        sb.AppendLine($"Time: {eventMessage.Timestamp}");
        sb.AppendLine($"Payload: {eventMessage.Payload}");
        sb.AppendLine("=================");
        logger.LogInformation(sb.ToString());
    }
}