using NLog;
using NLog.Web;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Base.StartupTasks;
using Oip.Notifications.Contexts;
using Oip.Notifications.Extensions;
using Oip.Notifications.Settings;

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
            builder.Services.AddSingleton<IBaseOipModuleAppSettings>(settings);
            builder.Services.AddSettingsToDependencyInjection(settings);
            builder.Services.AddNotificationData(settings);
            builder.AddDefaultHealthChecks();
            builder.AddDefaultAuthentication(settings);
            builder.AddOpenApi(settings);
            builder.Services.AddStartupTask<SwaggerGenerateWebClientStartupTask>();
            builder.Services.AddStartupRunner();
            builder.Services.AddCors();
            builder.AddControllersAndView();
            builder.AddLocalization();
            builder.Services.AddDataProtection<NotificationsDbContext>();
                        
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
            app.MigrateNotificationDatabase();
            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}