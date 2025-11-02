using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Services;

namespace Oip.Base.Runtime;

/// <summary>
/// Provides extension methods for the <see cref="IServiceCollection"/> interface to add startup tasks and runners.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the startup runner to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddStartupRunner(this IServiceCollection services)
    {
        return services
            .AddScoped<IStartupRunner, StartupRunner>()
            .AddHostedService<StartupRunnerHostedService>();
    }

    /// <summary>
    /// Adds a startup task to the dependency injection container.
    /// </summary>
    /// <typeparam name="TStartupTask">The type of the startup task to add.</typeparam>
    /// <param name="services">The service collection to add the startup task to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddStartupTask<TStartupTask>(this IServiceCollection services)
        where TStartupTask : class, IStartupTask
    {
        return services
            .AddScoped<TStartupTask>()
            .AddScoped<IStartupTask, TStartupTask>(sp => sp.GetRequiredService<TStartupTask>());
    }

    /// <summary>
    /// Adds a startup task to the dependency injection container.
    /// </summary>
    /// <typeparam name="TStartupTask">The type of the startup task to add.</typeparam>
    /// <param name="services">The service collection to add the startup task to.</param>
    /// <param name="factory">The factory func</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddStartupTask<TStartupTask>(this IServiceCollection services,
        Func<IServiceProvider, TStartupTask> factory) where TStartupTask : class, IStartupTask
    {
        return services
            .AddScoped(factory)
            .AddScoped<IStartupTask, TStartupTask>(sp => sp.GetRequiredService<TStartupTask>());
    }
}