using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oip.Example.Data.Contexts;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;

namespace Oip.Example.Data.Extensions;

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
    public static IServiceCollection AddExampleDataContext(this IServiceCollection services, string connectionString)
    {
        var connectionModel = ConnectionStringHelper.NormalizeConnectionString(connectionString);
        switch (connectionModel.Provider)
        {
            case XpoProvider.Postgres:
                return services.AddDbContext<ExampleDataContext>(option =>
                {
                    option.UseNpgsql(connectionModel.NormalizeConnectionString,
                        x =>
                        {
                            x.MigrationsHistoryTable(ExampleDataContext.MigrationHistoryTableName, ExampleDataContext.MigrationHistorySchemaName); 
                            x.MigrationsAssembly("Oip.Example.Data.Postgres");
                        });
                });
            case XpoProvider.MSSqlServer:
                return services.AddDbContext<ExampleDataContext>(option =>
                {
                    option.UseSqlServer(connectionModel.NormalizeConnectionString,
                        x =>
                        {
                            x.MigrationsHistoryTable(ExampleDataContext.MigrationHistoryTableName, ExampleDataContext.MigrationHistorySchemaName); 
                            x.MigrationsAssembly("Oip.Example.Data.SqlServer");
                        });
                });
            default:
                return services;
        }
    }
}