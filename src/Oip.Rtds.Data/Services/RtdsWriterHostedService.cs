using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Settings;

namespace Oip.Rtds.Data.Services;

/// <summary>
/// Background service that drains <see cref="RtdsWriteQueue"/> and writes tag values to ClickHouse
/// in batches over a long-lived connection.
/// </summary>
public sealed class RtdsWriterHostedService : BackgroundService
{
    private readonly RtdsWriteQueue _queue;
    private readonly RtdsContextFactory _contextFactory;
    private readonly ILogger<RtdsWriterHostedService> _logger;
    private readonly RtdsWriterSettings _settings;
    private RtdsContext _context = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="RtdsWriterHostedService"/> class.
    /// </summary>
    /// <param name="queue">The queue of values waiting to be written.</param>
    /// <param name="contextFactory">Factory producing the long-lived ClickHouse context.</param>
    /// <param name="appSettings">Application settings containing writer settings.</param>
    /// <param name="logger">Logger instance for logging operations.</param>
    public RtdsWriterHostedService(RtdsWriteQueue queue, RtdsContextFactory contextFactory,
        IRtdsAppSettings appSettings, ILogger<RtdsWriterHostedService> logger)
    {
        _queue = queue;
        _contextFactory = contextFactory;
        _logger = logger;
        _settings = appSettings.RtdsWriter;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "RTDS writer started, batch size {MaxBatchSize}, flush interval {FlushInterval} ms",
            _settings.MaxBatchSize, _settings.FlushIntervalMilliseconds);

        _context = _contextFactory.Create();
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!await WaitForValuesAsync(stoppingToken))
                    break;

                var batch = await ReadBatchAsync(stoppingToken);
                if (batch.Count > 0)
                    await FlushAsync(batch, stoppingToken);
            }
        }
        finally
        {
            await DrainAsync();
            await _context.DisposeAsync();
            _logger.LogInformation("RTDS writer stopped");
        }
    }

    /// <summary>
    /// Waits until values are available, keeping the flush window open for the configured interval.
    /// </summary>
    /// <param name="stoppingToken">Token signalled when the host is shutting down.</param>
    /// <returns>False when the queue is completed or the host is stopping.</returns>
    private async Task<bool> WaitForValuesAsync(CancellationToken stoppingToken)
    {
        try
        {
            return await _queue.Reader.WaitToReadAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// Accumulates a batch, keeping the flush window open until it is full or the interval elapses.
    /// </summary>
    /// <param name="stoppingToken">Token signalled when the host is shutting down.</param>
    /// <returns>The values to write.</returns>
    private async Task<List<InsertValueDto<double>>> ReadBatchAsync(CancellationToken stoppingToken)
    {
        var batch = ReadBatch();
        if (batch.Count == 0 || batch.Count >= _settings.MaxBatchSize)
            return batch;

        using var window = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        window.CancelAfter(TimeSpan.FromMilliseconds(_settings.FlushIntervalMilliseconds));
        try
        {
            while (batch.Count < _settings.MaxBatchSize && await _queue.Reader.WaitToReadAsync(window.Token))
            {
                while (batch.Count < _settings.MaxBatchSize && _queue.Reader.TryRead(out var value))
                    batch.Add(value);
            }
        }
        catch (OperationCanceledException)
        {
            // The flush window elapsed or the host is stopping, write what has been accumulated.
        }

        return batch;
    }

    /// <summary>
    /// Takes the values currently available in the queue, up to the configured batch size.
    /// </summary>
    /// <returns>The values to write.</returns>
    private List<InsertValueDto<double>> ReadBatch()
    {
        var batch = new List<InsertValueDto<double>>(Math.Min(_settings.MaxBatchSize, 1024));
        while (batch.Count < _settings.MaxBatchSize && _queue.Reader.TryRead(out var value))
            batch.Add(value);
        return batch;
    }

    /// <summary>
    /// Writes the batch to ClickHouse, one insert per tag value type.
    /// </summary>
    /// <param name="batch">The values to write.</param>
    /// <param name="stoppingToken">Token signalled when the host is shutting down.</param>
    /// <returns>Task representing the asynchronous write operation.</returns>
    private async Task FlushAsync(List<InsertValueDto<double>> batch, CancellationToken stoppingToken)
    {
        foreach (var group in batch.GroupBy(value => value.ValueType))
            await WriteWithRetryAsync(group.ToList(), stoppingToken);
    }

    /// <summary>
    /// Writes a single-type batch, retrying on failure and reopening the connection between attempts.
    /// </summary>
    /// <param name="values">The values to write.</param>
    /// <param name="stoppingToken">Token signalled when the host is shutting down.</param>
    /// <returns>Task representing the asynchronous write operation.</returns>
    private async Task WriteWithRetryAsync(List<InsertValueDto<double>> values, CancellationToken stoppingToken)
    {
        for (var attempt = 0; attempt <= _settings.MaxRetryCount; attempt++)
        {
            try
            {
                // The write itself is not cancelled on shutdown, so an accepted batch is not lost.
                await _context.InsertValues(values, CancellationToken.None);
                return;
            }
            catch (Exception e)
            {
                if (attempt == _settings.MaxRetryCount || stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(e, "Dropping {Count} {ValueType} values after {Attempts} write attempts",
                        values.Count, values[0].ValueType, attempt + 1);
                    return;
                }

                _logger.LogWarning(e, "Write of {Count} {ValueType} values failed, attempt {Attempt}",
                    values.Count, values[0].ValueType, attempt + 1);

                await ResetContextAsync();
                await Task.Delay(TimeSpan.FromMilliseconds(_settings.RetryDelayMilliseconds * (attempt + 1)),
                    CancellationToken.None);
            }
        }
    }

    /// <summary>
    /// Replaces the context, so the next attempt runs on a freshly opened connection.
    /// </summary>
    /// <returns>Task representing the asynchronous operation.</returns>
    private async Task ResetContextAsync()
    {
        try
        {
            await _context.DisposeAsync();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to dispose the ClickHouse context");
        }

        _context = _contextFactory.Create();
    }

    /// <summary>
    /// Writes the values left in the queue when the host is shutting down.
    /// </summary>
    /// <returns>Task representing the asynchronous operation.</returns>
    private async Task DrainAsync()
    {
        while (true)
        {
            var batch = ReadBatch();
            if (batch.Count == 0)
                return;

            _logger.LogInformation("Draining {Count} values on shutdown", batch.Count);
            await FlushAsync(batch, CancellationToken.None);
        }
    }
}
