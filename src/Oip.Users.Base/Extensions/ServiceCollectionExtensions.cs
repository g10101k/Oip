using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Minio;
using Oip.Base.Runtime;
using Oip.Base.Services;
using Oip.Base.Settings;
using Oip.Notifications.Base;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;
using Oip.Users.Base.Contexts;
using Oip.Users.Base.Data.Repositories;
using Oip.Users.Base.Notifications;
using Oip.Users.Base.Services;
using Oip.Users.Base.Settings;
using Oip.Users.Base.StartupTasks;
using IUserCacheRepository = Oip.Base.Services.IUserCacheRepository;
using UserService = Oip.Users.Base.Services.UserService;

namespace Oip.Users.Base.Extensions;

/// <summary>
/// Provides extension methods for configuring users module services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the User service to the dependency injection container,
    /// switching between Local and Remote implementations based on IsStandalone.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">The application settings.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddUserServiceProxy(this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
    {
        return settings.IsStandalone
            ? services.AddUsersModuleLocal(settings)
            : services.AddUsersModuleRemote(settings);
    }

    /// <summary>
    /// Registers the local users module implementation.
    /// </summary>
    public static IServiceCollection AddUsersModuleLocal(this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
    {
        if (services.All(x => x.ServiceType != typeof(DbContextOptions<UserContext>)))
        {
            services.AddUsersData(settings);
        }

        services.TryAddScoped<UserRepository>();
        services.TryAddScoped<IUserService, LocalUserService>();
        services.TryAddScoped<UserService>();
        services.AddUserPhotoStorage();
        services.TryAddScoped<KeycloakSyncService>();
        services.AddUserCacheRepository();
        services.AddStartupTask<KeycloakSyncStartupTask>();

        if (settings.IsStandalone)
        {
            services.AddUsersNotificationPublisherCore();
        }
        else
        {
            services.AddUsersNotificationPublisherRemote(settings);
        }

        return services;
    }

    /// <summary>
    /// Registers the remote users module implementation.
    /// </summary>
    public static IServiceCollection AddUsersModuleRemote(this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
    {
        services.AddGrpcClient<GrpcUserService.GrpcUserServiceClient>(options =>
        {
            options.Address = new Uri(settings.Services.OipUsers);
        });
        services.TryAddScoped<IUserService, RemoteUserService>();
        services.AddUserCacheRepository();
        return services;
    }

    private static IServiceCollection AddUserCacheRepository(this IServiceCollection services)
    {
        services.TryAddSingleton<UserCacheRepository>();
        services.TryAddSingleton<IUserCacheRepository>(sp => sp.GetRequiredService<UserCacheRepository>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, UserCacheRepositoryHostedService>());
        return services;
    }

    private static IServiceCollection AddUserPhotoStorage(this IServiceCollection services)
    {
        services.TryAddSingleton<IMinioClient>(sp =>
        {
            var settings = sp.GetRequiredService<UserPhotoStorageSettings>();
            var client = new MinioClient()
                .WithEndpoint(settings.Endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey);

            if (settings.UseSsl)
            {
                client = client.WithSSL();
            }

            return client.Build();
        });
        services.TryAddScoped<IUserPhotoStorage, MinioUserPhotoStorage>();
        return services;
    }

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

    /// <summary>
    /// Registers the users notification publisher against the remote notifications service.
    /// </summary>
    public static IServiceCollection AddUsersNotificationPublisherRemote(
        this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
    {
        services.AddGrpcClient<GrpcNotificationService.GrpcNotificationServiceClient>(options =>
        {
            options.Address = new Uri(settings.Services.OipNotifications);
        });
        services.AddScoped<INotificationServiceClient, GrpcNotificationServiceClientAdapter>();
        services.AddUsersNotificationPublisherCore();
        return services;
    }

    /// <summary>
    /// Registers common users notification publisher services.
    /// </summary>
    public static IServiceCollection AddUsersNotificationPublisherCore(this IServiceCollection services)
    {
        services.AddScoped<BaseNotificationService>();
        services.AddScoped<INotificationPublisher>(sp => sp.GetRequiredService<BaseNotificationService>());
        services.AddStartupTask<NotificationStartup>();
        return services;
    }
}