using StackExchange.Redis;

namespace Oip.Hitl.Services;

/// <summary>
/// Kind of <see cref="AgentEvent"/>.
/// </summary>
public enum AgentEventType
{
    /// <summary>
    /// An attempt of the activity started; the deltas that follow belong to it.
    /// </summary>
    Start,

    /// <summary>
    /// Text delta of the answer.
    /// </summary>
    Delta
}

/// <summary>
/// Event of an agent run read from its stream.
/// </summary>
/// <param name="Id">Id of the stream entry; reading continues after it.</param>
/// <param name="Type">Kind of the event.</param>
/// <param name="Text">Text of a <see cref="AgentEventType.Delta"/>.</param>
/// <param name="Attempt">Attempt of the activity of a <see cref="AgentEventType.Start"/>.</param>
public record AgentEvent(string Id, AgentEventType Type, string? Text, int Attempt);

/// <summary>
/// Redis streams through which the activities of an agent run pass its progress, e.g. the text deltas of the
/// answer, to the gateway request waiting for the run. A stream is read from its beginning, so unlike pub/sub no
/// events are lost when the reader starts after the activity.
/// </summary>
/// <param name="connectionString">Redis connection string; the stream fails on the first use when it is empty.</param>
/// <param name="ttl">How long the events of a run are kept.</param>
public sealed class AgentEventStream(string? connectionString, TimeSpan ttl) : IAsyncDisposable
{
    /// <summary>
    /// Stream position before the first event.
    /// </summary>
    public const string Beginning = "0-0";

    private const string TypeField = "type";
    private const string TextField = "text";
    private const string AttemptField = "attempt";
    private const string StartType = "start";
    private const string DeltaType = "delta";

    private readonly Lazy<Task<ConnectionMultiplexer>> _connection = new(() => ConnectAsync(connectionString));

    /// <summary>
    /// Key of the stream of an agent run.
    /// </summary>
    public static string GetKey(string runId) => $"oip:agent:{runId}";

    /// <summary>
    /// Publishes the start of an activity attempt and renews the expiration of the stream.
    /// </summary>
    public async Task PublishStartAsync(string key, int attempt)
    {
        var database = await GetDatabaseAsync();
        await database.StreamAddAsync(key, [new(TypeField, StartType), new(AttemptField, attempt)]);
        await database.KeyExpireAsync(key, ttl);
    }

    /// <summary>
    /// Publishes a text delta of the answer.
    /// </summary>
    public async Task PublishDeltaAsync(string key, string text)
    {
        var database = await GetDatabaseAsync();
        await database.StreamAddAsync(key, [new(TypeField, DeltaType), new(TextField, text)]);
    }

    /// <summary>
    /// Reads the events published after <paramref name="afterId"/>, oldest first; empty when there are none yet.
    /// </summary>
    /// <param name="key">Key of the stream.</param>
    /// <param name="afterId">Id of the last read event, or <see cref="Beginning"/>.</param>
    public async Task<IReadOnlyList<AgentEvent>> ReadAsync(string key, string afterId)
    {
        var database = await GetDatabaseAsync();
        var entries = await database.StreamReadAsync(key, afterId);
        return entries.Select(ToEvent).ToList();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_connection.IsValueCreated && _connection.Value.IsCompletedSuccessfully)
            await _connection.Value.Result.DisposeAsync();
    }

    private static AgentEvent ToEvent(StreamEntry entry)
    {
        var type = entry[TypeField] == StartType ? AgentEventType.Start : AgentEventType.Delta;
        var attempt = entry[AttemptField];
        return new AgentEvent(entry.Id.ToString(), type, entry[TextField], attempt.IsNull ? 0 : (int)attempt);
    }

    private async Task<IDatabase> GetDatabaseAsync() => (await _connection.Value).GetDatabase();

    private static async Task<ConnectionMultiplexer> ConnectAsync(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Redis is not configured for the agent gateway: set AgentGateway:RedisConnectionString");

        var options = ConfigurationOptions.Parse(connectionString);
        // Reconnects in the background instead of failing when Redis is not up yet.
        options.AbortOnConnectFail = false;
        return await ConnectionMultiplexer.ConnectAsync(options);
    }
}
