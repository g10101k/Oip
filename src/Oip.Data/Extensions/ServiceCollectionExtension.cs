using Microsoft.EntityFrameworkCore;
using Oip.Data.Contexts;
using Oip.Data.Repositories;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// ServiceCollection Extensions
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Add data context
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString"></param>
    /// <param name="migrationHistoryTableName"></param>
    /// <param name="migrationHistorySchemaName"></param>
    /// <returns></returns>
    public static IServiceCollection AddOipModuleContext(this IServiceCollection services, string connectionString,
        string migrationHistoryTableName = OipModuleContext.MigrationHistoryTableName,
        string migrationHistorySchemaName = OipModuleContext.SchemaName)
    {
        var connectionModel = ConnectionStringHelper.NormalizeConnectionString(connectionString);
        return services.AddDbContext<OipModuleContext>(option =>
                {
                    switch (connectionModel.Provider)
                    {
                        case XpoProvider.Postgres:
                            option.UseNpgsql(connectionModel.NormalizeConnectionString,
                                x =>
                                {
                                    x.MigrationsHistoryTable(migrationHistoryTableName, migrationHistorySchemaName);
                                });
                            break;
                        case XpoProvider.MSSqlServer:
                            option.UseSqlServer(connectionModel.NormalizeConnectionString,
                                x =>
                                {
                                    x.MigrationsHistoryTable(migrationHistoryTableName, migrationHistorySchemaName);
                                });
                            break;
                    }
                }
            )
            .AddScoped<ModuleRepository>();
    }
}