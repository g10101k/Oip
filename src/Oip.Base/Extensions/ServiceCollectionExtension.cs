using Microsoft.EntityFrameworkCore;
using Oip.Data.Contexts;
using Oip.Data.Postgres;
using Oip.Data.Repositories;
using Oip.Data.Sqlite;
using Oip.DataData.SqlServer;
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
                    Console.WriteLine("SQLite");

                    option.UseSqlite(connectionModel.NormalizeConnectionString, x =>
                    {
                        x.MigrationsAssembly(typeof(SqliteMarker).Assembly.GetName().Name!);
                        x.MigrationsHistoryTable(OipContext.MigrationHistoryTableName);
                    });
                    break;
                case XpoProvider.Postgres:
                    Console.WriteLine("Postgres");

                    option.UseNpgsql(connectionModel.NormalizeConnectionString, x =>
                    {
                        x.MigrationsAssembly(typeof(PostgresMarker).Assembly.GetName().Name!);
                        x.MigrationsHistoryTable(OipContext.MigrationHistoryTableName,
                            "public");
                    });
                    break;
                case XpoProvider.MSSqlServer:
                    Console.WriteLine("MSSqlServer");

                    option.UseSqlServer(connectionModel.NormalizeConnectionString, x =>
                    {
                        x.MigrationsAssembly(typeof(SqlServerMarker).Assembly.GetName().Name!);
                        x.MigrationsHistoryTable(OipContext.MigrationHistoryTableName,
                            "dbo");
                    });
                    break;
                default:
                    option.UseInMemoryDatabase(connectionModel.NormalizeConnectionString);
                    break;
            }
        })
        .AddScoped<FeatureRepository>();
    }
}