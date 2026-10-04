namespace Oip.Hitl.Services;

/// <summary>
/// Storage of the protected user tokens of the agent runs.
/// </summary>
public interface IAgentUserTokenStorage
{
    /// <summary>
    /// Returns the value of the run; <c>null</c> when there is none or it has expired.
    /// </summary>
    Task<string?> GetAsync(string runId);

    /// <summary>
    /// Sets the value of the run until the expiration.
    /// </summary>
    Task SetAsync(string runId, string value, DateTimeOffset expiresAt);

    /// <summary>
    /// Deletes the value of the run.
    /// </summary>
    Task DeleteAsync(string runId);
}

/// <summary>
/// Keeps the user tokens of the agent runs in Redis.
/// </summary>
public class RedisAgentUserTokenStorage(AgentRedisConnection redis, TimeProvider timeProvider) : IAgentUserTokenStorage
{
    /// <inheritdoc />
    public async Task<string?> GetAsync(string runId)
    {
        var database = await redis.GetDatabaseAsync();
        return await database.StringGetAsync(GetKey(runId));
    }

    /// <inheritdoc />
    public async Task SetAsync(string runId, string value, DateTimeOffset expiresAt)
    {
        var ttl = expiresAt - timeProvider.GetUtcNow();
        if (ttl <= TimeSpan.Zero) return;
        var database = await redis.GetDatabaseAsync();
        await database.StringSetAsync(GetKey(runId), value, ttl);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string runId)
    {
        var database = await redis.GetDatabaseAsync();
        await database.KeyDeleteAsync(GetKey(runId));
    }

    private static string GetKey(string runId) => $"oip:agent:{runId}:user-token";
}
