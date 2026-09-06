using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Runtime;
using Oip.Base.Settings;

namespace Oip.Base.Security.Connectivity;

/// <summary>
/// Registration of the secret connectivity probes.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the startup task running every registered <see cref="ISecretConnectivityProbe"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSecretConnectivityValidation(this IServiceCollection services)
    {
        services.AddStartupTask<SecretConnectivityStartupTask>();

        return services;
    }

    /// <summary>
    /// Adds a probe to the set executed at startup.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="factory">Factory creating the probe.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSecretConnectivityProbe(this IServiceCollection services,
        Func<IServiceProvider, ISecretConnectivityProbe> factory)
    {
        services.AddScoped(factory);

        return services;
    }

    /// <summary>
    /// Adds the probe reporting a Redis password that does not match the one Redis was started with.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">Authentication ticket store settings.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRedisSecretConnectivityProbe(this IServiceCollection services,
        AuthTicketStoreSettings settings)
    {
        return services.AddSecretConnectivityProbe(_ => new RedisSecretConnectivityProbe(settings));
    }
}
