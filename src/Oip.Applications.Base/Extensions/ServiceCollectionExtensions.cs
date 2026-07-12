using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oip.Applications.Base.Controllers;
using Oip.Applications.Base.Data.Contexts;
using Oip.Applications.Base.Data.Repositories;
using Oip.Applications.Base.Services;
using Oip.Applications.Base.StartupTasks;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;
using GrpcApplicationRegistryService = Oip.Applications.Base.Grpc.GrpcApplicationRegistryService;

namespace Oip.Applications.Base.Extensions;

/// <summary>
/// Provides extension methods for configuring application registry services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the application registry services.
    /// </summary>
    public static IServiceCollection AddApplicationsService(this IServiceCollection services, ISettings settings,
        AddingMode? addingMode = null)
    {
        var mode = addingMode ?? settings.ServiceAddingMode;

        switch (mode)
        {
            case AddingMode.Local:
                services.AddApplicationsData(settings);
                services.AddLocalServices();
                break;
            case AddingMode.Service:
                services.AddApplicationsData(settings)
                    .AddLocalServices()
                    .AddBaseServiceControllers()
                    .AddController<ApplicationsController>();
                services.AddGrpc();
                break;
            case AddingMode.Remote:
                services.AddGrpcClient<GrpcApplicationRegistryService.GrpcApplicationRegistryServiceClient>(options =>
                {
                    options.Address = new Uri(settings.Services.OipApplications);
                });
                services.TryAddScoped<IApplicationRegistryService, GrpcApplicationRegistryServiceClientAdapter>();
                services.AddStartupTask<ApplicationSelfRegistrationStartupTask>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return services;
    }

    /// <summary>
    /// Registers local application registry services.
    /// </summary>
    public static IServiceCollection AddLocalServices(this IServiceCollection services)
    {
        services.TryAddScoped<ApplicationRegistryRepository>();
        services.TryAddScoped<IApplicationRegistryService, LocalApplicationRegistryService>();
        services.AddStartupTask<ApplicationSelfRegistrationStartupTask>();

        return services;
    }

    /// <summary>
    /// Adds application registry data services.
    /// </summary>
    public static IServiceCollection AddApplicationsData(this IServiceCollection services, ISettings settings)
    {
        var connectionModel = ConnectionStringHelper.NormalizeConnectionString(settings.ConnectionString);
        switch (connectionModel.Provider)
        {
            case XpoProvider.Postgres:
                services.AddDbContext<ApplicationRegistryDbContext>(option =>
                {
                    option.UseNpgsql(connectionModel.NormalizeConnectionString,
                        x =>
                        {
                            x.MigrationsHistoryTable(
                                ApplicationRegistryDbContext.MigrationHistoryTableName,
                                ApplicationRegistryDbContext.SchemaName);
                        });
                });
                break;
            case XpoProvider.MSSqlServer:
                services.AddDbContext<ApplicationRegistryDbContext>(option =>
                {
                    option.UseSqlServer(connectionModel.NormalizeConnectionString,
                        x =>
                        {
                            x.MigrationsHistoryTable(
                                ApplicationRegistryDbContext.MigrationHistoryTableName,
                                ApplicationRegistryDbContext.SchemaName);
                        });
                });
                break;
            default:
                services.AddDbContext<ApplicationRegistryDbContext>(option =>
                {
                    option.UseInMemoryDatabase("Oip.Applications");
                });
                break;
        }

        return services;
    }
}
