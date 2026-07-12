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
        AddingMode? startupMode = null)
    {
        var mode = startupMode ?? settings.AddingMode;
        switch (mode)
        {
            case AddingMode.Local:
                services.AddNotificationData(settings);
                services.AddLocalServices(settings);
                services.AddSignalR();
                services.TryAddScoped<INotificationServiceClient, LocalNotificationServiceClient>();
                break;
            case AddingMode.Service:
                services.AddNotificationData(settings);
                services.AddLocalServices(settings);
                services.AddSignalR();
                services.AddBaseServiceControllers()
                    .AddController<NotificationController>();
                services.AddGrpc();
                break;
            case AddingMode.Remote:
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