using Microsoft.EntityFrameworkCore;
using Oip.Settings;
using Oip.Settings.Contexts;
using Oip.Settings.Enums;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// IServiceCollection extension
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Add application settings database context
    /// </summary>
    /// <param name="services"></param>
    /// <param name="appSettings"></param>
    /// <returns></returns>
    public static IServiceCollection AddAppSettingsDbContext(this IServiceCollection services, IAppSettings appSettings)
    {
        services.AddSingleton(appSettings.AppSettingsOptions);
        return services.AddDbContext<AppSettingsContext>(option =>
        {
            switch (appSettings.Provider)
            {
                case XpoProvider.SQLite:
                    option.UseSqlite(appSettings.NormalizedConnectionString);
                    break;
                case XpoProvider.Postgres:
                    option.UseNpgsql(appSettings.NormalizedConnectionString);
                    break;
                case XpoProvider.MSSqlServer:
                    option.UseSqlServer(appSettings.NormalizedConnectionString);
                    break;
                case XpoProvider.InMemoryDataStore:
                    option.UseInMemoryDatabase(appSettings.NormalizedConnectionString);
                    break;
                default:
                    throw new InvalidOperationException("Unknown provider");
            }
        });
    }
}