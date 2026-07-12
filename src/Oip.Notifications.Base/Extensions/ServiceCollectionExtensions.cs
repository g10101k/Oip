using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Services;
using Oip.Base.Settings;
using Oip.Notifications.Base.Controllers;
using Oip.Notifications.Base.Data.Contexts;
using Oip.Notifications.Base.Data.Repositories;
using Oip.Notifications.Base.Services;
using Oip.Notifications.Base.Startups;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;

namespace Oip.Notifications.Base.Extensions;

/// <summary>
/// Provides extension methods for configuring notifications module services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers notifications for local composition.
    /// </summary>
    public static IServiceCollection AddNotificationsService(this IServiceCollection services, ISettings settings,
        StartupMode? remote = null)
    {
        var mode = remote ?? settings.StartupMode;
        
        if (mode is StartupMode.Standalone or StartupMode.Service)
        {
            // Нужно регать данные Context + Repository
            services.AddNotificationData(settings);
            // Внутренние сервисы обеспечивающие бизнес логику
            services.AddLocalServices(settings);
            services.AddSignalR();
        }

        switch (mode)
        {
            case StartupMode.Standalone:
                // Нужно регистрировать локальные имплементации которые подменяют вызовы grpc
                services.TryAddScoped<INotificationServiceClient, LocalNotificationServiceClient>();
                break;
            case StartupMode.Service:
                services
                    .AddBaseServiceControllers()
                    .AddController<NotificationController>();
                // Нужно зарегистрировать grpc сервисы чтобы они работали
                services.AddGrpc();
                break;
            case StartupMode.Remote:
                // Нужно зарегистрировать grpc клиенты
                services.AddGrpcClient<GrpcNotificationService.GrpcNotificationServiceClient>(options =>
                {
                    options.Address = new Uri(settings.Services.OipNotifications);
                });

                services.TryAddScoped<INotificationServiceClient, GrpcNotificationServiceClientAdapter>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        return services;
    }

    public static IServiceCollection AddLocalServices(this IServiceCollection services, ISettings settings)
    {
        services.TryAddSingleton<CryptService>();
        services.TryAddSingleton<ChannelService>();
        services.TryAddScoped<NotificationService>();
        services.AddStartupTask<ChannelStartup>();
        return services;
    }

    /// <summary>
    /// Adds notification data services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">The application settings.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddNotificationData(this IServiceCollection services, ISettings settings)
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

        return services;
    }
}