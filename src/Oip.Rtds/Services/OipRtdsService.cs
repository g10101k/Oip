using System.Collections.Concurrent;
using Grpc.Core;
using Oip.Rtds.Grpc;
using Oip.Rts.Extensions;

namespace Oip.Rts.Services;

public class OipRtdsService : RtdsService.RtdsServiceBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly ConcurrentDictionary<string, IServerStreamWriter<EventMessage>> Subscribers = new();
    private static readonly object Lock = new();
    private static int _eventCounter;

    public OipRtdsService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public override async Task Subscribe(SubscribeRequest request, IServerStreamWriter<EventMessage> responseStream,
        ServerCallContext context)
    {
        var clientId = request.ClientId;

        // Добавляем подписчика
        Subscribers[clientId] = responseStream;
        Console.WriteLine($@"Client {clientId} subscribed to events: {string.Join(", ", request.EventTypes)}");

        try
        {
            // Ждем, пока клиент не отключится
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, context.CancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"Client {clientId} disconnected");
        }
        finally
        {
            // Удаляем подписчика при отключении
            Subscribers.TryRemove(clientId, out _);
        }
    }

    public override Task<PublishResponse> Publish(PublishRequest request, ServerCallContext context)
    {
        var eventMessage = new EventMessage
        {
            EventId = GenerateEventId(),
            EventType = request.EventType,
            Payload = request.Payload,
            Timestamp = DateTime.UtcNow.ToString("O")
        };

        // Отправляем событие всем подписчикам
        BroadcastEvent(eventMessage);

        return Task.FromResult(new PublishResponse
        {
            Success = true,
            Message = $"Event {eventMessage.EventId} published"
        });
    }


    public override Task<GetTagsResponse> GetTags(GetTagsRequest request, ServerCallContext context)
    {
        return _scopeFactory.ExecuteAsync<TagService, GetTagsResponse>(x => x.GetTagsByInterfaceId(request));
    }

    private void BroadcastEvent(EventMessage eventMessage)
    {
        foreach (var subscriber in Subscribers)
        {
            try
            {
                subscriber.Value.WriteAsync(eventMessage).Wait();
                Console.WriteLine($@"Event {eventMessage.EventId} sent to client {subscriber.Key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Failed to send event to client {subscriber.Key}: {ex.Message}");
                Subscribers.TryRemove(subscriber.Key, out _);
            }
        }
    }

    private string GenerateEventId()
    {
        lock (Lock)
        {
            return $"event_{++_eventCounter}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        }
    }
}