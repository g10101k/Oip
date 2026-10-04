using StackExchange.Redis;

namespace Oip.Hitl.Services;

/// <summary>
/// Redis connection of the agent runs, shared by <see cref="AgentEventStream"/> and <see cref="AgentUserTokenStore"/>.
/// </summary>
/// <param name="connectionString">Redis connection string; the connection fails on the first use when it is empty.</param>
public sealed class AgentRedisConnection(string? connectionString) : IAsyncDisposable
{
    private readonly Lazy<Task<ConnectionMultiplexer>> _connection = new(() => ConnectAsync(connectionString));

    /// <summary>
    /// Returns the database, connecting on the first call.
    /// </summary>
    public async Task<IDatabase> GetDatabaseAsync() => (await _connection.Value).GetDatabase();

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_connection.IsValueCreated && _connection.Value.IsCompletedSuccessfully)
            await _connection.Value.Result.DisposeAsync();
    }

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
