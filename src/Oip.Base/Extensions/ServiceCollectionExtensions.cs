using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oip.Base.Controllers;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Base.StartupTasks;

namespace Oip.Base.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebClientGenerationStartupTask(this IServiceCollection services,
        ISettings settings) =>
#pragma warning disable CS0618 // Type or member is obsolete
        services.GenerateWebClientStartupTask(settings);
#pragma warning restore CS0618 // Type or member is obsolete

    [Obsolete("Use AddWebClientGenerationStartupTask instead")]
    public static IServiceCollection GenerateWebClientStartupTask(this IServiceCollection services,
        ISettings settings)
    {
        if (!settings.GenerateWebClient)
            return services;

        services.Configure<HostOptions>(options =>
        {
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });
        services.AddStartupTask<SwaggerGenerateWebClientStartupTask>();
        return services;
    }

    /// <summary>
    /// Base controllers for service discussions, users, notification and other
    /// </summary>
    public static IServiceCollection AddBaseServiceControllers(this IServiceCollection services)
    {
        services
            .AddController<CryptController>()
            .AddController<SecurityController>();

        return services;
    }

    /// <summary>
    /// Base controllers for application 
    /// </summary>
    public static IServiceCollection AddApplicationControllers(this IServiceCollection services)
    {
        services
            .AddBaseServiceControllers()
            .AddController<FolderModuleController>()
            .AddController<IframeModuleController>()
            .AddController<MenuController>()
            .AddController<ModuleController>()
            .AddController<ProxySettingsController>()
            .AddController<ExtensionsController>()
            .AddController<ExtensionModulesController>();

        return services;
    }
}