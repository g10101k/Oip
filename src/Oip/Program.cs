using NLog;
using NLog.Web;
using Microsoft.EntityFrameworkCore;
using Oip.Applications.Base.Extensions;
using Oip.Base.Data.Extensions;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Controllers;
using Oip.Settings;
using Oip.Demo.TableQueryDemo;
using Oip.Discussions.Base.Extensions;
using Oip.Extensions;
using Oip.Notifications.Base.Extensions;
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
            builder.Services.AddSingleton<ISettings>(settings);
            builder.Services.AddSettingsToDependencyInjection(settings);
            builder.Services.AddOipModuleContext(settings.ConnectionString);
            builder.Services.AddDbContext<DemoCustomerTableContext>(options =>
                options.UseInMemoryDatabase("CustomerTableDemo"));
            builder.Services.AddDefaultHealthChecks();
            builder.Services.AddDefaultAuthentication(settings);
            builder.Services.AddOpenApi(settings);
            builder.Services.AddWebClientGenerationStartupTask(settings);
            builder.Services.AddStartupRunner();
            builder.Services.AddHttpClient();
            builder.Services.AddCors(settings);
            builder.Services.AddDataProtection(settings);
            builder.Services.AddForwardedHeaders(settings);

            builder.Services.AddOipLocalization();
            builder.Services.AddOpenTelemetry(settings);
            builder.Services.AddControllersAndView();
            builder.Services.AddUserService(settings);
            builder.Services.AddDiscussionsService(settings);
            builder.Services.AddNotificationsService(settings);
            builder.Services.AddApplicationsService(settings);

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
            app.UseCors();
            app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
            app.MapOpenApi(settings);
            app.MapFallbackToFile("index.html");
            app.MapOpenTelemetry(settings);
            app.UseApplicationsService(settings);
            app.UseUsersService(settings);
            app.UseDiscussionsService(settings);
            app.UseNotificationsService(settings);
            
            app.MigrateOipModuleDatabase();
            app.MigrateDemoCustomerTableContext();
            
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