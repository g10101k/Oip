using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Services;
using Oip.Base.Settings;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Repositories;
using Oip.Notifications.Hubs;
using Oip.Notifications.Services;
using Oip.Notifications.Startups;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;
using Oip.Users.Base;
using Oip.Users.Notifications;

namespace Oip.Notifications.Extensions;

/// <summary>
/// Provides extension methods for configuring notifications module services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers notifications for local composition.
    /// </summary>
    public static IServiceCollection AddNotificationsModuleLocal(this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
    {
        services.AddNotificationsModuleCore(settings);
        services.TryAddScoped<INotificationServiceClient, LocalNotificationServiceClient>();
        services.AddDataProtection<NotificationsDbContext>();
        services.TryAddActivatedSingleton<CryptService>();
        return services;
    }

    /// <summary>
    /// Registers notifications for distributed composition.
    /// </summary>
    public static IServiceCollection AddNotificationsModuleRemote(this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
    {
        return services.AddNotificationsModuleCore(settings);
    }

    private static IServiceCollection AddNotificationsModuleCore(this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
    {
        if (services.All(x => x.ServiceType != typeof(DbContextOptions<NotificationsDbContext>)))
        {
            services.AddNotificationData(settings);
        }

        services.TryAddSingleton<UserCacheRepository>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, UserCacheRepositoryHostedService>());
        services.TryAddSingleton<ChannelService>();
        services.TryAddScoped<NotificationService>();
        services.TryAddScoped<NotificationHub>();
        services.AddStartupTask<ChannelStartup>();
        return services;
    }

    /// <summary>
    /// Adds notification data services to the dependency injection container.
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
        services.AddScoped<NotificationUserRepository>();
        services.AddScoped<NotificationDeliveryRepository>();

        return services;
    }
}
