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
    public static IServiceCollection AddOipModuleContext(this IServiceCollection services, string connectionString,
        string migrationHistoryTableName = OipModuleContext.MigrationHistoryTableName,
        string migrationHistorySchemaName = OipModuleContext.SchemaName)
    {
        return services
            .AddOipBasedContext<OipModuleContext>(connectionString, migrationHistoryTableName,
                migrationHistorySchemaName)
            .AddScoped<ModuleRepository>();
    }

    /// <summary>
    /// Adds and configures the Oip module context and module repository to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The connection string for the database.</param>
    /// <param name="migrationHistoryTableName">The name of the migration history table (default: "__OipModuleMigrationHistory").</param>
    /// <param name="migrationHistorySchemaName">The schema name for the migration history table (default: "oip").</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddOipBasedContext<T>(this IServiceCollection services, string connectionString,
        string migrationHistoryTableName = OipModuleContext.MigrationHistoryTableName,
        string migrationHistorySchemaName = OipModuleContext.SchemaName)
        where T : DbContext
    {
        var connectionModel = ConnectionStringHelper.NormalizeConnectionString(connectionString);
        // Регистрируем DbContext с лямбдой конфигурации
        return services.AddDbContext<T>((serviceProvider, options) =>
        {
            switch (connectionModel.Provider)
            {
                case XpoProvider.Postgres:
                    options.UseNpgsql(connectionModel.NormalizeConnectionString,
                        x => x.MigrationsHistoryTable(migrationHistoryTableName, migrationHistorySchemaName));
                    break;
                case XpoProvider.MSSqlServer:
                    options.UseSqlServer(connectionModel.NormalizeConnectionString,
                        x => x.MigrationsHistoryTable(migrationHistoryTableName, migrationHistorySchemaName));
                    break;
                case XpoProvider.InMemoryDataStore:
                    options.UseInMemoryDatabase(typeof(T).Name);
                    break;
                case XpoProvider.SQLite:
                    throw new InvalidOperationException("SQLite provider is not supported");
                default:
                    throw new InvalidOperationException($"Invalid provider in connection string {connectionString}");
            }
        });
    }
}