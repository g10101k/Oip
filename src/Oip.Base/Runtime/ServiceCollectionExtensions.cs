using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Services;
using Oip.Core.HostedServices;

namespace Oip.Base.Runtime;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStartupRunner(this IServiceCollection services)
    {
        return services
            .AddScoped<IStartupRunner, StartupRunner>()
            .AddHostedService<StartupRunnerHostedService>();
    }

    public static IServiceCollection AddStartupTask<TStartupTask>(this IServiceCollection services)
        where TStartupTask : class, IStartupTask
    {
        return services
            .AddScoped<TStartupTask>()
            .AddScoped<IStartupTask, TStartupTask>(sp => sp.GetRequiredService<TStartupTask>());
    }

    public static IServiceCollection AddStartupTask<TStartupTask>(this IServiceCollection services,
        Func<IServiceProvider, TStartupTask> factory) where TStartupTask : class, IStartupTask
    {
        return services
            .AddScoped(factory)
            .AddScoped<IStartupTask, TStartupTask>(sp => sp.GetRequiredService<TStartupTask>());
    }
}