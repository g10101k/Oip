using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oip.Rtds.Data.Contexts;
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
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static IServiceCollection AddRtdsDataContext(this IServiceCollection services, string connectionString)
    {
        var connectionModel = ConnectionStringHelper.NormalizeConnectionString(connectionString);
        switch (connectionModel.Provider)
        {
            case XpoProvider.Postgres:
                return services.AddDbContext<RtdsMetaContext>(option =>
                {
                    option.UseNpgsql(connectionModel.NormalizeConnectionString,
                        x =>
                        {
                            x.MigrationsHistoryTable(RtdsMetaContext.MigrationHistoryTableName, RtdsMetaContext.SchemaName); 
                            x.MigrationsAssembly("Oip.Rtds.Data.Postgres");
                        });
                });
            case XpoProvider.MSSqlServer:
                return services.AddDbContext<RtdsMetaContext>(option =>
                {
                    option.UseSqlServer(connectionModel.NormalizeConnectionString,
                        x =>
                        {
                            x.MigrationsHistoryTable(RtdsMetaContext.MigrationHistoryTableName, RtdsMetaContext.SchemaName); 
                            x.MigrationsAssembly("Oip.Rtds.Data.SqlServer");
                        });
                });
            default:
                return services;
        }
    }
}