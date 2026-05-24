using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oip.Applications.Base;
using Oip.Applications.Base.Grpc;
using Oip.Applications.Data;
using Oip.Applications.Services;
using Oip.Applications.Base.StartupTasks;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;

namespace Oip.Applications.Extensions;

/// <summary>
/// Provides extension methods for configuring application registry services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the local application registry implementation.
    /// </summary>
    public static IServiceCollection AddApplicationsModuleLocal(this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
    {
        services.AddApplicationsData(settings);
        services.TryAddScoped<ApplicationRegistryRepository>();
        services.TryAddScoped<IApplicationRegistryService, LocalApplicationRegistryService>();
        services.AddStartupTask<ApplicationSelfRegistrationStartupTask>();
        return services;
    }

    /// <summary>
    /// Adds application registry data services.
    /// </summary>
    public static IServiceCollection AddApplicationsData(this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
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