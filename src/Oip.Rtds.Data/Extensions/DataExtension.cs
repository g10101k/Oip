using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Repositories;
using Oip.Rtds.Data.Settings;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;

namespace Oip.Rtds.Data.Extensions;

/// <summary>
/// Data Example Context
/// </summary>
public static class DataExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static IServiceCollection AddRtdsData(this IServiceCollection services, AppSettings settings)
    {
        var connectionModel = ConnectionStringHelper.NormalizeConnectionString(settings.ConnectionString);
        services.AddScoped<RtdsContext>();
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
                            x.MigrationsAssembly("Oip.Rtds.Data.Postgres");
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
                            x.MigrationsAssembly("Oip.Rtds.Data.SqlServer");
                        });
                });
                break;
        }
        services.AddScoped<RtdsContext>();
        services.AddScoped<TagRepository>();
        return services;
    }
}