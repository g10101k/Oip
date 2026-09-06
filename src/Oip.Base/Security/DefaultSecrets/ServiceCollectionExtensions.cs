using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Oip.Base.Runtime;

namespace Oip.Base.Security.DefaultSecrets;

/// <summary>
/// Registration of the default secret validation.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the cached validator together with the <c>Security</c> options section. Safe to call more than once.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    private static IServiceCollection AddDefaultSecretsValidator(this IServiceCollection services)
    {
        services.AddOptions<DefaultSecretsOptions>().BindConfiguration(DefaultSecretsOptions.SectionName);
        services.TryAddSingleton<DefaultSecretsValidator>();

        return services;
    }

    /// <summary>
    /// Adds the readiness health check reporting settings that still hold a secret shipped with the repository.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <returns>The updated health checks builder.</returns>
    public static IHealthChecksBuilder AddDefaultSecretsCheck(this IHealthChecksBuilder builder)
    {
        builder.Services.AddDefaultSecretsValidator();

        return builder.AddCheck<DefaultSecretsHealthCheck>(DefaultSecretsHealthCheck.Name,
            HealthStatus.Degraded, [DefaultSecretsHealthCheck.Tag]);
    }

    /// <summary>
    /// Adds the startup task that reports secret settings still holding a value shipped with the repository.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddDefaultSecretsValidation(this IServiceCollection services)
    {
        services.AddDefaultSecretsValidator();
        services.AddStartupTask<DefaultSecretsStartupTask>();

        return services;
    }
}
