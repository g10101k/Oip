using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Settings;
using Oip.Users.Base;
using Oip.Users.Contexts;
using Oip.Users.Repositories;
using Oip.Users.Services;

namespace Oip.Users.Extensions;

/// <summary>
/// Provides extension methods for configuring the User service proxy.
/// </summary>
public static class UserServiceProxyExtensions
{
    /// <summary>
    /// Adds the User service to the dependency injection container, 
    /// switching between Local and Remote implementations based on IsStandalone.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">The application settings.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddUserServiceProxy(this IServiceCollection services, IBaseOipModuleAppSettings settings)
    {
        return settings.IsStandalone
            ? services.AddUsersModuleLocal(settings)
            : services.AddUsersModuleRemote(settings);
    }

    /// <summary>
    /// Registers the local users module implementation.
    /// </summary>
    public static IServiceCollection AddUsersModuleLocal(this IServiceCollection services, IBaseOipModuleAppSettings settings)
    {
        if (!services.Any(x => x.ServiceType == typeof(DbContextOptions<UserContext>)))
        {
            services.AddUsersData(settings);
        }

        services.TryAddScoped<UserRepository>();
        services.TryAddScoped<IUserService, LocalUserService>();
        return services;
    }

    /// <summary>
    /// Registers the remote users module implementation.
    /// </summary>
    public static IServiceCollection AddUsersModuleRemote(this IServiceCollection services, IBaseOipModuleAppSettings settings)
    {
        services.AddGrpcClient<GrpcUserService.GrpcUserServiceClient>(options =>
        {
            options.Address = new Uri(settings.Services.OipUsers);
        });
        services.TryAddScoped<IUserService, RemoteUserService>();
        return services;
    }
}
