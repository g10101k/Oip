using System.Collections.Concurrent;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Base.Services;

/// <summary>
/// Service for writing buffered data to the RTDS service with compression and retry capabilities.
/// </summary>
public class BufferWriterService
{
    private readonly RtdsService.RtdsServiceClient _client;
    private readonly ILogger<BufferWriterService> _logger;
    private readonly CompressService _compressService;
    private readonly TagCacheService _cacheService;
    private readonly ConcurrentQueue<WriteDataRequest> _bufferQueue = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(10);
    private readonly Timer _retryTimer;

    /// <summary>
    /// Service for writing buffered data to the RTDS service with compression and retry capabilities.
    /// </summary>
    /// <param name="client">The RTDS service client for communication with the server.</param>
    /// <param name="logger">Logger for recording service operations and errors.</param>
    /// <param name="compressService">Service for handling data compression before writing.</param>
    /// <param name="cacheService">Service for managing tag cache operations.</param>
    public BufferWriterService(RtdsService.RtdsServiceClient client, ILogger<BufferWriterService> logger,
        CompressService compressService, TagCacheService cacheService)
    {
        _client = client;
        _logger = logger;
        _compressService = compressService;
        _cacheService = cacheService;
        _retryTimer = new Timer(async void (_) => await TryFlushBufferAsync(), null, _retryInterval, _retryInterval);
    }

    /// <summary>
    /// Writes data to the server based on compression settings.
    /// </summary>
    /// <param name="calculateResults">The results to be written to the server.</param>
    /// <exception cref="InvalidOperationException">Thrown when the tag is not found or invalid.</exception>
    public async Task WriteToService(IEnumerable<CalculateResult> calculateResults)
    {
        var writeDataRequest = await _compressService.CompressFilterData(calculateResults);

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
            await _client.WriteDataAsync(request);
            _cacheService.UpdateValues(request);
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
}