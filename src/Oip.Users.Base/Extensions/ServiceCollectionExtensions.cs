using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Minio;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Services;
using Oip.Base.Settings;
using Oip.Notifications.Base;
using Oip.Notifications.Base.Services;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;
using Oip.Users.Base.Contexts;
using Oip.Users.Base.Controllers;
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
    /// switching between Standalone, Service and Remote implementations based on StartupMode.
    /// </summary>
    public static IServiceCollection AddUserService(this IServiceCollection services,
        ISettings settings, StartupMode? overrideMode = null)
    {
        var mode = settings.StartupMode;
        if (overrideMode is not null)
            mode = overrideMode.Value;

        if (mode is StartupMode.Standalone or StartupMode.Service)
        {
            services.AddUsersData(settings);
            services.AddLocalServices(settings);
        }

        switch (mode)
        {
            case StartupMode.Standalone:
                // Do nothing, all controllers
                break;
            case StartupMode.Service:
                services
                    .AddBaseServiceControllers()
                    .AddController<UsersController>()
                    .AddController<UserProfileController>();
                services.AddGrpc();
                break;
            case StartupMode.Remote:
                services.AddGrpcClient<GrpcUserService.GrpcUserServiceClient>(options =>
                {
                    options.Address = new Uri(settings.Services.OipUsers);
                });
                services.TryAddScoped<IUserService, RemoteUserService>();
                services.AddUserCacheRepository();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return services;
    }

    /// <summary>
    /// Registers local users business services.
    /// </summary>
    public static IServiceCollection AddLocalServices(this IServiceCollection services, ISettings settings)
    {
        services.TryAddScoped<UserRepository>();
        services.TryAddScoped<IUserService, LocalUserService>();
        services.TryAddScoped<UserService>();
        services.AddUserPhotoStorage();
        services.TryAddScoped<KeycloakSyncService>();
        services.AddUserCacheRepository();
        services.AddStartupTask<KeycloakSyncStartupTask>();
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
    public static IServiceCollection AddUsersData(this IServiceCollection services, ISettings settings)
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