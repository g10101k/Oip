using NLog;
using NLog.Web;
using Oip.Applications.Base;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Services;
using Oip.Base.Settings;
using Oip.Notifications.Extensions;
using Oip.Notifications.Settings;
using Oip.Users.Extensions;

namespace Oip.Notifications;

internal static class Program
{
    public static void Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        try
        {
            var settings = AppSettings.Initialize(args, false, true);

            if (settings.IsStandalone)
            {
                logger.Warn("Oip.Notifications service is configured to run in Standalone mode. Run the main Oip host instead.");
                return;
            }

            var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgramArguments);

            builder.AddNlog();
            builder.Services.AddSingleton<IBaseOipModuleAppSettings>(settings);
            builder.Services.AddSettingsToDependencyInjection(settings);
            builder.AddDefaultHealthChecks();
            builder.AddDefaultAuthentication(settings);
            builder.AddOpenApi(settings);
            builder.Services.AddApplicationsModuleRemote(settings);
            builder.Services.AddUsersModuleRemote(settings);
            
            builder.Services.AddSingleton<CryptService>();
            builder.Services.AddNotificationsModuleRemote(settings);
            builder.Services.AddStartupRunner();
            builder.Services.AddCors();
            builder.Services.AddGrpc().AddJsonTranscoding();
            builder.Services.AddGrpcSwagger();
            builder.AddControllersAndView();
            builder.AddLocalization();
            builder.Services.AddOipDataProtection(settings);
            builder.Services.AddSignalR();
            builder.AddOpenTelemetry(settings);

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
            app.AddNotificationsModuleLocal();
            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}
