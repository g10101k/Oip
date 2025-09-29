using Microsoft.EntityFrameworkCore;
using Oip.Base.Settings;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;
using Oip.Users.Contexts;

namespace Oip.Users.Extensions;

/// <summary>
/// Provides extension methods for configuring user data services.
/// </summary>
public static class DataExtension
{
    /// <summary>
    /// Add user data services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">The application settings.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddUsersData(this IServiceCollection services, IBaseOipModuleAppSettings settings)
    {
        var connectionModel = ConnectionStringHelper.NormalizeConnectionString(settings.ConnectionString);
        switch (connectionModel.Provider)
        {
            case XpoProvider.Postgres:
                services.AddDbContext<UserContext>(option =>
                {
                    option.UseNpgsql(connectionModel.NormalizeConnectionString,
                        x =>
                        {
                            x.MigrationsHistoryTable(UserContext.MigrationHistoryTableName, UserContext.SchemaName);
                        });
                });
                break;
            case XpoProvider.MSSqlServer:
                services.AddDbContext<UserContext>(option =>
                {
                    option.UseSqlServer(connectionModel.NormalizeConnectionString,
                        x =>
                        {
                            x.MigrationsHistoryTable(UserContext.MigrationHistoryTableName, UserContext.SchemaName);
                        });
                });
                break;
        }

        return services;
    }
}