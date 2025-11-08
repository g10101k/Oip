using System.Collections.Concurrent;
using Grpc.Core;
using Oip.Base.Extensions;
using Oip.Rtds.Grpc;

namespace Oip.Rts.Services;

/// <summary>
/// Implements the Rtds service for handling real-time data streams.
/// </summary>
public class RtdsService : Rtds.Grpc.RtdsService.RtdsServiceBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly ConcurrentDictionary<string, IServerStreamWriter<EventMessage>> Subscribers = new();
    private static readonly object Lock = new();
    private static int _eventCounter;

    /// <summary>
    /// Implements the Rtds service for handling real-time data streams.
    /// </summary>
    public RtdsService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Subscribes a client to real-time data streams for specified event types.
    /// </summary>
    /// <param name="request">The subscription request containing the client ID and event types.</param>
    /// <param name="responseStream">The stream to send event messages to the client.</param>
    /// <param name="context">The context for the gRPC call.</param>
    public override async Task Subscribe(SubscribeRequest request, IServerStreamWriter<EventMessage> responseStream,
        ServerCallContext context)
    {
        var clientId = request.ClientId;

        Subscribers[clientId] = responseStream;
        Console.WriteLine($@"Client {clientId} subscribed to events: {string.Join(", ", request.EventTypes)}");

        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, context.CancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($@"Client {clientId} disconnected");
        }
        finally
        {
            Subscribers.TryRemove(clientId, out _);
        }
    }

    /// <summary>
    /// Publishes an event to the real-time data stream.
    /// </summary>
    /// <param name="request">The publish request containing event type and payload.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>A publish response indicating the success of the operation.</returns>
    public override Task<PublishResponse> Publish(PublishRequest request, ServerCallContext context)
    {
        var eventMessage = new EventMessage
        {
            EventId = GenerateEventId(),
            EventType = request.EventType,
            Payload = request.Payload,
            Timestamp = DateTime.UtcNow.ToString("O")
        };

        BroadcastEvent(eventMessage);

        return Task.FromResult(new PublishResponse
        {
            Success = true,
            Message = $"Event {eventMessage.EventId} published"
        });
    }


    /// <summary>
    /// Retrieves tags based on the provided request.
    /// </summary>
    /// <param name="request">The request containing interface ID.</param>
    /// <param name="context">The server call context.</param>
    /// <returns>A response containing the retrieved tags.</returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task<WriteDataResponse> WriteData(WriteDataRequest request, ServerCallContext context)
    {
        return _scopeFactory.ExecuteAsync<TagService, WriteDataResponse>(x => x.WriteData(request));
    }

    private string GenerateEventId()
    {
        lock (Lock)
        {
            return $"event_{++_eventCounter}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        }
    }
}