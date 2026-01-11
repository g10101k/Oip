using Microsoft.EntityFrameworkCore;
using Oip.Base.Settings;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Repositories;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;

namespace Oip.Notifications.Extensions;

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
    public static IServiceCollection AddNotificationData(this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
    {
        var connectionModel = ConnectionStringHelper.NormalizeConnectionString(settings.ConnectionString);
        switch (connectionModel.Provider)
        {
            case XpoProvider.Postgres:
                services.AddDbContext<NotificationsDbContext>(option =>
                {
                    option.UseNpgsql(connectionModel.NormalizeConnectionString,
                        x =>
                        {
                            x.MigrationsHistoryTable(NotificationsDbContext.MigrationHistoryTableName,
                                NotificationsDbContext.SchemaName);
                        });
                });
                break;
            case XpoProvider.MSSqlServer:
                services.AddDbContext<NotificationsDbContext>(option =>
                {
                    option.UseSqlServer(connectionModel.NormalizeConnectionString,
                        x =>
                        {
                            x.MigrationsHistoryTable(NotificationsDbContext.MigrationHistoryTableName,
                                NotificationsDbContext.SchemaName);
                        });
                });
                break;
            default:
                services.AddDbContext<NotificationsDbContext>(option =>
                {
                    option.UseInMemoryDatabase("Oip.Notifications");
                });
                break;
        }

        services.AddScoped<NotificationDeliveryRepository>();
        services.AddScoped<NotificationTypeRepository>();
        services.AddScoped<NotificationChannelRepository>();
        services.AddScoped<NotificationTemplateRepository>();
        services.AddScoped<UserNotificationPreferenceRepository>();
        services.AddScoped<NotificationRepository>();
        services.AddScoped<NotificationDeliveryRepository>();

        return services;
    }
}