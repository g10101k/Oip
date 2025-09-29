using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using Oip.Base.Extensions;
using Oip.Users.Extensions;
using Oip.Users.Repositories;
using Oip.Users.Services;
using Oip.Users.Settings;

namespace Oip.Users;

internal static class Program
{
    public static void Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        try
        {
            var settings = AppSettings.Initialize(args, false, true);
            var builder = OipModuleApplication.CreateShellBuilder(settings);
            builder.AddKeycloakClients(settings);
            builder.Services.AddUsersData(settings);
            // Repositories
            builder.Services.AddScoped<UserRepository>();

            // Services
            builder.Services.AddScoped<KeycloakService>();
            builder.Services.AddScoped<UserSyncService>();

            // Background service
            builder.Services.AddHostedService<BackgroundSyncService>();

            // HTTP Client
            builder.Services.AddHttpClient();
            var app = builder.BuildApp(settings);
            app.MigrateUserDatabase();
            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}