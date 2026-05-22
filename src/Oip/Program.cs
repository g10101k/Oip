using NLog;
using NLog.Web;
using Microsoft.EntityFrameworkCore;
using Oip.Applications.Base;
using Oip.Applications.Data;
using Oip.Applications.Extensions;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Base.StartupTasks;
using Oip.Data.Extensions;
using Oip.Discussions.Extensions;
using Oip.Notifications.Extensions;
using Oip.Settings;
using Oip.Users.Extensions;
using Oip.Demo.TableQueryDemo;
using Oip.Extensions;

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
            if (settings.OpenApi.Any(x => !string.IsNullOrWhiteSpace(x.GenerateCommand)))
                builder.Services.AddStartupTask<SwaggerGenerateWebClientStartupTask>();
            builder.Services.AddStartupRunner();
            builder.Services.AddCors();
            builder.AddControllersAndView();
            builder.AddLocalization();
            builder.AddOpenTelemetry(settings);

            if (settings.IsStandalone)
            {
                builder.Services.AddApplicationsModuleLocal(settings);

                builder.Services.AddUsersModuleLocal(settings);

                builder.Services.AddDiscussionsModuleLocal(settings);

                builder.Services.AddNotificationsModuleLocal(settings);

                builder.Services.AddSignalR();
                builder.Services.AddGrpc();
            }
            else
            {
                builder.Services.AddApplicationsModuleRemote(settings);
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
            app.UseOipCsrfProtection();
            app.UseAuthorization();
            app.UseCors(options => options.AllowAnyOrigin());
            app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
            app.MapOpenApi(settings);
            app.MapFallbackToFile("index.html");
            app.MapOpenTelemetry(settings);

            app.MigrateOipModuleDatabase();
            app.MigrateDatabase<ApplicationRegistryDbContext>();
            app.MigrateDemoCustomerTableContext();

            if (settings.IsStandalone)
            {
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