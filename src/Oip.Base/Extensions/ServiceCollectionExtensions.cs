using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Base.StartupTasks;

namespace Oip.Base.Extensions;

public static class ServiceCollectionExtensions
{
    public static void GenerateWebClientStartupTask(this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
    {
        if (!settings.GenerateWebClient) return;
        
        services.Configure<HostOptions>(options =>
        {
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });
        services.AddStartupTask<SwaggerGenerateWebClientStartupTask>();
    }
}