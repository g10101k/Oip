using NLog;
using NLog.Web;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Base.StartupTasks;
using Oip.Data.Extensions;
using Oip.Discussions.Extensions;
using Oip.Discussions.Settings;
using Oip.Users.Extensions;

namespace Oip.Discussions;

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
                logger.Warn(
                    "Oip.Discussions service is configured to run in Standalone mode. Run the main Oip host instead.");
                return;
            }

            var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgramArguments);

            builder.AddNlog();
            builder.Services.AddSingleton<IBaseOipModuleAppSettings>(settings);
            builder.Services.AddSettingsToDependencyInjection(settings);
            builder.Services.AddOipModuleContext(settings.ConnectionString);
            builder.AddDefaultHealthChecks();
            builder.AddDefaultAuthentication(settings);
            builder.AddOpenApi(settings);
            builder.Services.AddStartupTask<SwaggerGenerateWebClientStartupTask>();
            builder.Services.AddStartupRunner();
            builder.Services.AddCors();
            builder.Services.AddUsersModuleRemote(settings);
            builder.Services.AddDiscussionsModuleRemote(settings);
            builder.AddControllersAndView();
            builder.AddLocalization();

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

            app.MigrateOipModuleDatabase();
            app.AddDiscussions(settings);

            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}