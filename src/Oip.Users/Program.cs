using NLog;
using NLog.Web;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Notifications;
using Oip.Notifications.Base;
using Oip.Users.Extensions;
using Oip.Users.Notifications;
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
            builder.Services.AddStartupRunner();
            builder.Services.AddSingleton<BaseNotificationService>();
            builder.Services.AddStartupTask<NotificationStartup>();
            builder.Services.AddSettingsToDependencyInjection(settings);
            builder.Services.AddUsersData(settings);
            builder.Services.AddGrpcClient<GrpcNotificationService.GrpcNotificationServiceClient>(x =>
            {
                x.Address = new Uri(settings.Services.OipNotifications);
            });

            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<UserSyncService>();
            builder.Services.AddHostedService<KeycloakSyncBackgroundService>();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddGrpc();
            var app = builder.BuildApp(settings);
            app.MapGrpcService<UserService>();
            app.MigrateUserDatabase();
            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}