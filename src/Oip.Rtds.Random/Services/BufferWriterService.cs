using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Oip.Rtds.Base;
using Oip.Rtds.Grpc;
using System.Collections.Concurrent;

namespace Oip.Rtds.Random.Services;

public class BufferWriterService
{
    private readonly TagCacheService _tagCacheService;
    private readonly RtdsService.RtdsServiceClient _client;
    private readonly ILogger<BufferWriterService> _logger;
    
    private readonly ConcurrentQueue<WriteDataRequest> _bufferQueue = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(10);
    private readonly Timer _retryTimer;

    public BufferWriterService(TagCacheService tagCacheService,
        RtdsService.RtdsServiceClient client,
        ILogger<BufferWriterService> logger)
    {
        _tagCacheService = tagCacheService;
        _client = client;
        _logger = logger;
        _retryTimer = new Timer(async void (_) => await TryFlushBufferAsync(), null, _retryInterval, _retryInterval);
    }


    /// <summary>
    /// Writes data to the server based on compression settings.
    /// </summary>
    /// <param name="calculateResults">The results to be written to the server.</param>
    /// <exception cref="InvalidOperationException">Thrown when the tag is not found or invalid.</exception>
    public async Task WriteToService(IEnumerable<CalculateResult> calculateResults)
    {
        var enumerable = calculateResults as CalculateResult[] ?? calculateResults.ToArray();
        var writeDataRequest = new WriteDataRequest();

        foreach (var result in enumerable)
        {
            try
            {
                if (!_tagCacheService.TryGetTag(result.TagId, out var tag) || tag is null)
                    throw new InvalidOperationException($"Tag with id {result.TagId} not found");

                if (ShouldWriteValue(result, tag))
                {
                    writeDataRequest.Tags.Add(PrepareDataSend(result, tag));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing tag with ID {TagId}", result.TagId);
            }
        }

        if (writeDataRequest.Tags.Count == 0)
            return;

        await SendOrBufferAsync(writeDataRequest);
    }

    /// <summary>
    /// Attempt to send or buffer on error.
    /// </summary>
    private async Task SendOrBufferAsync(WriteDataRequest request)
    {
        await _sendLock.WaitAsync();
        try
        {
            try
            {
                await _client.WriteDataAsync(request);
            }
            catch (RpcException ex)
            {
                _logger.LogWarning(ex, "RPC failed, buffering data ({Count} tags)", request.Tags.Count);
                _bufferQueue.Enqueue(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending data");
                _bufferQueue.Enqueue(request);
            }
        }
        finally
        {
            _sendLock.Release();
        }
    }

    /// <summary>
    /// Periodic attempt to flush the buffer.
    /// </summary>
    private async Task TryFlushBufferAsync()
    {
        if (_bufferQueue.IsEmpty)
            return;

        await _sendLock.WaitAsync();
        try
        {
            var requeued = new List<WriteDataRequest>();
            while (_bufferQueue.TryDequeue(out var req))
            {
                try
                {
                    await _client.WriteDataAsync(req);
                    _logger.LogInformation("Flushed buffered data ({Count} tags)", req.Tags.Count);
                }
                catch (RpcException ex)
                {
                    _logger.LogWarning(ex, "Retry failed, keeping data in buffer");
                    requeued.Add(req);
                }
            }

            // Requeue failed attempts
            foreach (var r in requeued)
                _bufferQueue.Enqueue(r);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    /// <summary>
    /// Determines whether a value should be written to RTDS based on the compression settings.
    /// </summary>
    /// <param name="calculateResult">The calculated result to check.</param>
    /// <param name="tag">The tag associated with the result.</param>
    /// <returns><c>true</c> if the value should be written, <c>false</c> otherwise.</returns>
    private static bool ShouldWriteValue(CalculateResult calculateResult, TagResponse tag)
    {
        if (!tag.Compressing)
            return true;

        var lastTime = tag.ValueTime.ToDateTimeOffset();
        var deltaTime = calculateResult.Time - lastTime;
        var deltaValue = Math.Abs(Convert.ToDouble(tag.Value) - Convert.ToDouble(calculateResult.Value));

        // Skip if the time difference is too small
        if (deltaTime.TotalMilliseconds < tag.CompressionMinTime)
            return false;

        // Write if the time difference exceeds the maximum or the value change exceeds the error threshold
        return deltaTime.TotalMilliseconds > tag.CompressionMaxTime || deltaValue > calculateResult.Error;
    }

    /// <summary>
    /// Prepares data for sending to the server.
    /// </summary>
    /// <param name="calculateResult">The calculated result to send.</param>
    /// <param name="tagResponse">The tag associated with the result.</param>
    /// <returns>A <see cref="WriteDataTag"/> containing the prepared data.</returns>
    private static WriteDataTag PrepareDataSend(CalculateResult calculateResult, TagResponse tagResponse)
    {
        return new WriteDataTag
        {
            Id = tagResponse.Id,
            Value = calculateResult.Value.ToString(),
            Time = Timestamp.FromDateTimeOffset(calculateResult.Time)
        };
    }
}
