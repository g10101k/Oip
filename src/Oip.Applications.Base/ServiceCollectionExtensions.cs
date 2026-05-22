using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oip.Applications.Base.Grpc;
using Oip.Applications.Base.StartupTasks;
using Oip.Base.Runtime;
using Oip.Base.Settings;

namespace Oip.Applications.Base;

/// <summary>
/// Provides shared application registry client registration extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the remote application registry implementation.
    /// </summary>
    public static IServiceCollection AddApplicationsModuleRemote(
        this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
    {
        services.AddGrpcClient<GrpcApplicationRegistryService.GrpcApplicationRegistryServiceClient>(options =>
        {
            options.Address = new Uri(settings.Services.OipApplications);
        });
        services.TryAddScoped<IApplicationRegistryService, GrpcApplicationRegistryServiceClientAdapter>();
        services.AddStartupTask<ApplicationSelfRegistrationStartupTask>();
        return services;
    }
}
