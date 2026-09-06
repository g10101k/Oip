using Microsoft.Extensions.Logging;
using Oip.Base.Runtime;

namespace Oip.Base.Security.Connectivity;

/// <summary>
/// Runs the registered secret connectivity probes and turns a rejected credential into a message naming the
/// configuration key to fix. Never fails startup: a dependency that is merely unreachable is a routine
/// condition, and the services degrade gracefully without it.
/// </summary>
/// <param name="probes">The registered probes.</param>
/// <param name="logger">The logger.</param>
public sealed class SecretConnectivityStartupTask(
    IEnumerable<ISecretConnectivityProbe> probes,
    ILogger<SecretConnectivityStartupTask> logger) : IStartupTask
{
    /// <inheritdoc />
    public int Order => int.MinValue + 1;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        foreach (var probe in probes)
        {
            SecretConnectivityResult result;
            try
            {
                result = await probe.ProbeAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogDebug(exception, "Secret connectivity probe for {Dependency} failed unexpectedly.",
                    probe.Dependency);
                continue;
            }

            switch (result.Status)
            {
                case SecretConnectivityStatus.AuthenticationFailed:
                    logger.LogError("{SecretConnectivityFinding}", result.Message);
                    break;
                case SecretConnectivityStatus.Unavailable:
                    logger.LogWarning("{SecretConnectivityFinding}", result.Message);
                    break;
                default:
                    logger.LogDebug("{SecretConnectivityFinding}", result.Message);
                    break;
            }
        }
    }
}
