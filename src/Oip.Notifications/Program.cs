using NLog;
using NLog.Web;
using Oip.Applications.Base.Extensions;
using Oip.Base.Controllers;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Services;
using Oip.Base.Settings;
using Oip.Notifications.Base.Controllers;
using Oip.Notifications.Base.Extensions;
using Oip.Notifications.Base.Settings;
using Oip.Users.Base.Extensions;

namespace Oip.Notifications;

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
            builder.Services.AddDefaultHealthChecks();
            builder.Services.AddDefaultAuthentication(settings);
            builder.Services.AddOpenApi(settings);
            builder.Services.AddApplicationsService(settings);
            builder.Services.AddUserService(settings);

            builder.Services.AddStartupRunner();
            builder.Services.AddCors(settings);
            builder.Services.AddForwardedHeaders(settings);
            builder.Services.AddControllersAndView();
            builder.Services
                .AddBaseServiceControllers()
                .AddNotificationsService(settings);
            
            builder.Services.AddOipLocalization();
            builder.Services.AddDataProtection(settings);
            builder.Services.AddSignalR();
            builder.Services.AddOpenTelemetry(settings);

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
            app.UseNotificationsService(settings);
            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}
