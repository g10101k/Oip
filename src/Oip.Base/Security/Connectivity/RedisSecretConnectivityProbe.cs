using Oip.Base.Settings;
using StackExchange.Redis;

namespace Oip.Base.Security.Connectivity;

/// <summary>
/// Checks that Redis accepts the password carried by the authentication ticket store connection string.
/// </summary>
/// <param name="settings">Authentication ticket store settings.</param>
public sealed class RedisSecretConnectivityProbe(AuthTicketStoreSettings settings) : ISecretConnectivityProbe
{
    /// <inheritdoc />
    public string Dependency => "Redis";

    /// <inheritdoc />
    public string ConfigHint =>
        "the password in 'SecurityService:AuthTicketStore:RedisConnectionString' (REDIS_PASSWORD)";

    /// <inheritdoc />
    public async Task<SecretConnectivityResult> ProbeAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = settings.RedisConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
            return SecretConnectivityResult.Skipped(Dependency, "no connection string is configured.");

        try
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = true;
            options.ConnectRetry = 1;

            using var connection = await ConnectionMultiplexer.ConnectAsync(options);
            await connection.GetDatabase().PingAsync();

            return SecretConnectivityResult.Ok(Dependency);
        }
        catch (Exception exception)
        {
            return SecretConnectivityFailureClassifier.Classify(Dependency, ConfigHint, exception);
        }
    }
}
