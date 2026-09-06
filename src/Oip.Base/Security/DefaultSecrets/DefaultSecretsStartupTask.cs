using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oip.Base.Runtime;

namespace Oip.Base.Security.DefaultSecrets;

/// <summary>
/// Reports secret bearing settings that still hold the values shipped with the repository.
/// </summary>
public sealed class DefaultSecretsStartupTask(
    DefaultSecretsValidator validator,
    IHostEnvironment environment,
    IOptions<DefaultSecretsOptions> options,
    ILogger<DefaultSecretsStartupTask> logger) : IStartupTask
{
    /// <inheritdoc />
    public int Order => int.MinValue;

    /// <inheritdoc />
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Value.ValidateDefaultSecrets)
        {
            logger.LogDebug("Default secret validation is disabled by configuration.");
            return Task.CompletedTask;
        }

        var report = validator.Validate();

        if (!report.HasFindings)
        {
            logger.LogInformation("Default secret validation passed, {CheckedCount} secret settings checked.",
                report.CheckedCount);
            return Task.CompletedTask;
        }

        var failOnDefaults = environment.IsProduction() && options.Value.FailOnDefaultSecrets;

        foreach (var finding in report.Findings)
        {
            var message = finding.Describe(environment.ApplicationName);
            if (environment.IsProduction())
                logger.LogError("{DefaultSecretFinding}", message);
            else
                logger.LogWarning("{DefaultSecretFinding}", message);
        }

        if (!failOnDefaults)
            return Task.CompletedTask;

        var keys = string.Join(", ", report.Findings.Select(x => x.ConfigKey));
        throw new InvalidOperationException(
            $"Startup refused: {report.Findings.Count} secret setting(s) still hold a shipped default or are " +
            $"missing ({keys}). Set '{DefaultSecretsOptions.SectionName}:{nameof(DefaultSecretsOptions.FailOnDefaultSecrets)}' " +
            "to false to start anyway.");
    }
}
