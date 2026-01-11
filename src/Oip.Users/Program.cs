using NLog;
using NLog.Web;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.StartupTasks;
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
            var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgramArguments);
            builder.AddNlog();
            builder.AddDefaultHealthChecks();
            builder.AddDefaultAuthentication(settings);
            builder.AddOpenApi(settings);
            builder.Services.AddStartupTask<SwaggerGenerateWebClientStartupTask>();
            builder.Services.AddStartupRunner();
            builder.Services.AddSingleton(settings);
            builder.Services.AddScoped<UserService>();
            builder.Services.AddCors();
            builder.AddControllersAndView();
            builder.AddLocalization();
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
            
            var app = builder.Build();
            app.AddRequestLocalization();
            app.AddExceptionHandler();
            app.MapDefaultEndpoints();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors(options => options.AllowAnyOrigin());
            app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
            app.MapOpenApi(settings);
            app.MapFallbackToFile("index.html");
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