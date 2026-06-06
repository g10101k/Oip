using NLog;
using NLog.Web;
using Microsoft.EntityFrameworkCore;
using Oip.Applications.Base;
using Oip.Applications.Base.Extensions;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Data.Extensions;
using Oip.Discussions.Extensions;
using Oip.Notifications.Extensions;
using Oip.Settings;
using Oip.Users.Extensions;
using Oip.Demo.TableQueryDemo;
using Oip.Extensions;
using ServiceCollectionExtensions = Oip.Applications.Base.Extensions.ServiceCollectionExtensions;

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
            builder.AddControllersAndView();
            builder.AddLocalization();
            builder.AddOpenTelemetry(settings);

            if (settings.IsStandalone)
            {
                builder.Services.AddUsersModuleLocal(settings);
                builder.Services.AddDiscussionsModuleLocal(settings);
                builder.Services.AddNotificationsModuleLocal(settings);
                builder.Services.AddApplicationsModuleLocal(settings);
                builder.Services.AddSignalR();
                builder.Services.AddGrpc();
            }
            else
            {
                builder.Services.AddUsersModuleRemote(settings);
                builder.Services.AddApplicationsModuleRemote(settings);
                builder.Services.AddNotificationsModuleRemote(settings);
            }

            var app = builder.Build();

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
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}