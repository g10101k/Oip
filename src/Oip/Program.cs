using NLog;
using NLog.Web;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Base.Services;
using Oip.Data.Extensions;
using Oip.Discussions.Extensions;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Extensions;
using Oip.Settings;
using Oip.Users.Notifications;
using Oip.Users.Services;
using Oip.Users.Extensions;
using Oip.Demo.TableQueryDemo;
using Oip.Extensions;
using GrpcUserServiceImpl = Oip.Users.Services.UserService;

namespace Oip;

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
            builder.Services.AddSingleton<IBaseOipModuleAppSettings>(settings);
            builder.Services.AddSettingsToDependencyInjection(settings);
            builder.Services.AddOipModuleContext(settings.ConnectionString);
            builder.Services.AddDbContext<DemoCustomerTableContext>(options =>
                options.UseInMemoryDatabase("CustomerTableDemo"));
            builder.AddDefaultHealthChecks();
            builder.AddDefaultAuthentication(settings);
            builder.AddOpenApi(settings);
            builder.Services.AddStartupTask<SwaggerGenerateWebClientStartupTask>();
            builder.Services.AddStartupRunner();
            builder.Services.AddCors();
            builder.AddControllersAndView();
            builder.AddLocalization();
            builder.AddOpenTelemetry(settings);

            if (settings.IsStandalone)
            {
                builder.Services.AddUsersModuleLocal(settings);
                builder.Services.AddScoped<GrpcUserServiceImpl>();
                builder.Services.AddScoped<UserSyncService>();
                builder.Services.AddSingleton<INotificationPublisher, NoOpNotificationPublisher>();

                builder.Services.AddDiscussionsModuleLocal(settings);

                builder.Services.AddNotificationsModuleLocal(settings);
                builder.Services.AddDataProtection<NotificationsDbContext>();
                builder.Services.AddSignalR();
                builder.Services.AddGrpc().AddJsonTranscoding();
                builder.Services.AddGrpcSwagger();
                builder.Services.AddSingleton<CryptService>();
            }
            else
            {
                builder.Services.AddUsersModuleRemote(settings);
            }

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
            app.MapOpenTelemetry(settings);

            app.MigrateOipModuleDatabase();
            app.MigrateDemoCustomerTableContext();

            if (settings.IsStandalone)
            {
                app.MigrateUserDatabase();
                app.AddDiscussions(settings);
                app.MigrateNotificationDatabase();
                app.MapNotificationsModule();
            }

            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}