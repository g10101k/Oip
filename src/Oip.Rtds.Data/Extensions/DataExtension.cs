using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Settings;
using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Repositories;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;

namespace Oip.Rtds.Data.Extensions;

/// <summary>
/// Data Example Context
/// </summary>
public static class DataExtension
{
    /// <summary>
    /// Adds RTDS data services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">The application settings.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddRtdsData(this IServiceCollection services, IBaseOipModuleAppSettings settings)
    {
        var connectionModel = ConnectionStringHelper.NormalizeConnectionString(settings.ConnectionString);
        switch (connectionModel.Provider)
        {
            case XpoProvider.Postgres:
                services.AddDbContext<RtdsMetaContext>(option =>
                {
                    option.UseNpgsql(connectionModel.NormalizeConnectionString,
                        x =>
                        {
                            x.MigrationsHistoryTable(RtdsMetaContext.MigrationHistoryTableName,
                                RtdsMetaContext.SchemaName);
                        });
                });
                break;
            case XpoProvider.MSSqlServer:
                services.AddDbContext<RtdsMetaContext>(option =>
                {
                    option.UseSqlServer(connectionModel.NormalizeConnectionString,
                        x =>
                        {
                            x.MigrationsHistoryTable(RtdsMetaContext.MigrationHistoryTableName,
                                RtdsMetaContext.SchemaName);
                        });
                });
                break;
        }
        services.AddScoped<RtdsContext>();
        services.AddScoped<TagRepository>();
        services.AddScoped<RtdsRepository>();
        return services;
    }
}