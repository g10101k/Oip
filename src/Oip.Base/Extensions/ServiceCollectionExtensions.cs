using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Base.StartupTasks;

namespace Oip.Base.Extensions;

public static class ServiceCollectionExtensions
{
    public static void GenerateWebClientStartupTask(this IServiceCollection serviceCollection,
        IBaseOipModuleAppSettings settings)
    {
        if (settings.OpenApi.Any(x => !string.IsNullOrWhiteSpace(x.GenerateCommand) && x.Publish))
            serviceCollection.AddStartupTask<SwaggerGenerateWebClientStartupTask>();
    }
}