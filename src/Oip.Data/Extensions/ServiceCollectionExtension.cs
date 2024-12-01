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
    /// <returns></returns>
    public static IServiceCollection AddData(this IServiceCollection services, string connectionString)
    {
        var connectionModel = ConnectionStringHelper.NormalizeConnectionString(connectionString);
        return services.AddDbContext<OipContext>(option =>
                {
                    switch (connectionModel.Provider)
                    {
                        case XpoProvider.SQLite:
                            option.UseSqlite(connectionModel.NormalizeConnectionString,
                                x => { x.MigrationsHistoryTable(OipContext.MigrationHistoryTableName); });
                            break;
                        case XpoProvider.Postgres:
                            option.UseNpgsql(connectionModel.NormalizeConnectionString,
                                x => { x.MigrationsHistoryTable(OipContext.MigrationHistoryTableName); });
                            break;
                        case XpoProvider.MSSqlServer:
                            option.UseSqlServer(connectionModel.NormalizeConnectionString,
                                x => { x.MigrationsHistoryTable(OipContext.MigrationHistoryTableName); });
                            break;
                        default:
                            option.UseInMemoryDatabase(connectionModel.NormalizeConnectionString);
                            break;
                    }
                }
            )
            .AddScoped<FeatureRepository>();
    }
}