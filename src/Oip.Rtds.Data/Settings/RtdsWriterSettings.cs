namespace Oip.Rtds.Data.Settings;

/// <summary>
/// Settings of the background writer that batches tag values before inserting them into ClickHouse.
/// </summary>
public class RtdsWriterSettings
{
    /// <summary>
    /// Maximum number of values kept in the in-memory queue.
    /// When the queue is full, producers are throttled until the writer frees space.
    /// </summary>
    public int QueueCapacity { get; set; } = 500_000;

    /// <summary>
    /// Maximum number of values written to ClickHouse in a single insert.
    /// </summary>
    public int MaxBatchSize { get; set; } = 50_000;

    /// <summary>
    /// Maximum time a batch is accumulated before it is flushed, in milliseconds.
    /// </summary>
    public int FlushIntervalMilliseconds { get; set; } = 1_000;

    /// <summary>
    /// Number of additional attempts to write a batch before it is dropped.
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Delay between write attempts, in milliseconds. Grows linearly with the attempt number.
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 500;
}
