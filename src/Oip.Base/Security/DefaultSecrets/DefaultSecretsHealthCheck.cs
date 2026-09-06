using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Oip.Base.Security.DefaultSecrets;

/// <summary>
/// Reports settings that still hold a secret shipped with the repository as a degraded readiness state.
/// </summary>
public class DefaultSecretsHealthCheck(DefaultSecretsValidator validator) : IHealthCheck
{
    /// <summary>
    /// Name the health check is registered under.
    /// </summary>
    public const string Name = "default-secrets";

    /// <summary>
    /// Tag the health check is registered with.
    /// </summary>
    public const string Tag = "ready";

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var report = validator.Report;

        if (!report.HasFindings)
            return Task.FromResult(HealthCheckResult.Healthy(
                $"{report.CheckedCount} secret settings checked, none holds a shipped default."));

        var data = report.Findings.ToDictionary(x => x.ConfigKey, object (x) => x.Kind.ToString());

        var description = $"{report.Findings.Count} of {report.CheckedCount} secret settings still hold a value " +
                          "shipped with the repository or are missing: " +
                          string.Join(", ", report.Findings.Select(x => x.ConfigKey));

        return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description, data: data));
    }
}
