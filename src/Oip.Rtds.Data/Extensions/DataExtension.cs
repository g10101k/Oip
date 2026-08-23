using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Settings;
using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Repositories;
using Oip.Settings.Enums;

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
    public static IServiceCollection AddRtdsData(this IServiceCollection services, ISettings settings)
    {
        var connectionModel = settings.ConnectionString;
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
                    option.EnableSensitiveDataLogging(connectionModel.SensitiveDataLogging);
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
                    option.EnableSensitiveDataLogging(connectionModel.SensitiveDataLogging);
                });
                break;
            case XpoProvider.InMemoryDataStore:
                services.AddDbContext<RtdsMetaContext>(option =>
                {
                    option.UseInMemoryDatabase("Oip.Rtds");
                    option.EnableSensitiveDataLogging(connectionModel.SensitiveDataLogging);
                });
                break;
            case XpoProvider.SQLite:
                throw new InvalidOperationException("SQLite provider is not supported");
            default:
                throw new InvalidOperationException(
                    $"Invalid provider `{Enum.GetName(connectionModel.Provider)}` in connection string");
        }
        services.AddScoped<RtdsContext>();
        services.AddScoped<TagRepository>();
        services.AddScoped<RtdsRepository>();
        return services;
    }
}