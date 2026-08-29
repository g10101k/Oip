using System.Threading.Channels;
using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Settings;

namespace Oip.Rtds.Data.Services;

/// <summary>
/// In-memory queue of tag values waiting to be written to ClickHouse by <see cref="RtdsWriterHostedService"/>.
/// </summary>
public sealed class RtdsWriteQueue
{
    private readonly Channel<InsertValueDto<double>> _channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="RtdsWriteQueue"/> class.
    /// </summary>
    /// <param name="appSettings">Application settings containing writer settings.</param>
    public RtdsWriteQueue(IRtdsAppSettings appSettings)
    {
        var settings = appSettings.RtdsWriter;
        _channel = Channel.CreateBounded<InsertValueDto<double>>(
            new BoundedChannelOptions(settings.QueueCapacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            });
    }

    /// <summary>
    /// Gets the reader used by the background writer to drain the queue.
    /// </summary>
    public ChannelReader<InsertValueDto<double>> Reader => _channel.Reader;

    /// <summary>
    /// Enqueues values for writing. Completes when all values are accepted by the queue,
    /// which throttles the caller while the queue is full.
    /// </summary>
    /// <param name="values">Values to enqueue.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Task representing the asynchronous enqueue operation.</returns>
    public async Task EnqueueAsync(IReadOnlyList<InsertValueDto<double>> values,
        CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < values.Count; i++)
            await _channel.Writer.WriteAsync(values[i], cancellationToken);
    }

    /// <summary>
    /// Marks the queue as complete, so the background writer stops after draining the remaining values.
    /// </summary>
    public void Complete() => _channel.Writer.TryComplete();
}
