using NLog;
using NLog.Web;
using Microsoft.EntityFrameworkCore;
using Oip.Api.Controllers;
using Oip.Applications.Base.Controllers;
using Oip.Applications.Base.Extensions;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Controllers;
using Oip.Data.Extensions;
using Oip.Settings;
using Oip.Demo.TableQueryDemo;
using Oip.Discussions.Base.Controllers;
using Oip.Discussions.Base.Extensions;
using Oip.Extensions;
using Oip.Notifications.Base.Controllers;
using Oip.Notifications.Base.Extensions;
using Oip.Users.Base.Controllers;
using Oip.Users.Base.Extensions;

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
            builder.Services.GenerateWebClientStartupTask(settings);
            builder.Services.AddStartupRunner();
            builder.Services.AddCors();
            builder.Services.AddOipDataProtection(settings);
            builder.AddOipForwardedHeaders(settings);
            builder.AddControllersAndView();

            builder.AddLocalization();
            builder.AddOpenTelemetry(settings);

            if (settings.IsStandalone)
            {
                builder.Services.AddUsersModuleLocal(settings);
                builder.Services.AddDiscussionsModuleLocal(settings);
                builder.Services.AddNotificationsModuleLocal(settings);
                builder.Services.AddApplicationsModuleLocal(settings);
                builder.Services
                    .AddController<DiscussionController>()
                    .AddController<KeycloakEventsController>()
                    .AddController<NotificationController>()
                    .AddController<UserProfileController>()
                    .AddController<UsersController>();
                builder.Services.AddSignalR();
                builder.Services.AddGrpc();
            }
            else
            {
                builder.Services.AddUsersModuleRemote(settings);
                builder.Services.AddApplicationsModuleRemote(settings);
                builder.Services.AddNotificationsModuleRemote(settings);
            }

            builder.Services
                .AddController<CryptController>()
                .AddController<FolderModuleController>()
                .AddController<IframeModuleController>()
                .AddController<MenuController>()
                .AddController<ModuleController>()
                .AddController<ProxySettingsController>()
                .AddController<SecurityController>()
                .AddController<ApplicationsController>()
                .AddController<CustomerModuleController>()
                .AddController<DashboardModuleController>()
                .AddController<WeatherForecastModuleController>();

            var app = builder.Build();

            app.UseOipForwardedHeaders();
            app.AddRequestLocalization();
            app.AddExceptionHandler();
            app.MapDefaultEndpoints();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseOipCsrfProtection();
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
                app.UseOipApplications();
                app.AddUserModuleLocal();
                app.AddDiscussionsModuleLocal();
                app.AddNotificationsModuleLocal();
            }

            app.Run();
        }
        catch (OperationCanceledException)
        {
            logger.Info("Application execution cancelled, see logs for details.");
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}
