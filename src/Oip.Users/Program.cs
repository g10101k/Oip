using NLog;
using NLog.Web;
using Oip.Applications.Base.Extensions;
using Oip.Base.Controllers;
using Oip.Base.Extensions;
using Oip.Base.Settings;
using Oip.Users.Base.Controllers;
using Oip.Users.Base.Extensions;
using Oip.Users.Base.Services;
using Oip.Users.Base.Settings;

namespace Oip.Users;

internal static class Program
{
    public static void Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        try
        {
            var settings = AppSettings.Initialize(args, false, true);

            // Check if running in standalone mode (should only run as microservice)
            if (settings.IsStandalone)
            {
                logger.Warn("Oip.Users service is configured to run in Standalone mode. " +
                            "This service should only run in Microservices mode. " +
                            "Consider running the main Oip application instead.");
                return;
            }

            var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgramArguments);
            builder.AddNlog();
            builder.AddDefaultHealthChecks();
            builder.AddDefaultAuthentication(settings);
            builder.AddOpenApi(settings);
            builder.Services.AddSingleton<IBaseOipModuleAppSettings>(settings);
            builder.Services.AddSettingsToDependencyInjection(settings);
            builder.Services.AddApplicationsModuleRemote(settings);
            builder.Services.AddSingleton(settings);
            builder.Services.AddCors();
            builder.AddOipForwardedHeaders(settings);
            builder.AddControllersAndView();
            builder.Services
                .AddController<ProxySettingsController>()
                .AddController<SecurityController>()
                .AddController<UserProfileController>()
                .AddController<KeycloakEventsController>()
                .AddController<UsersController>();
            builder.AddLocalization();
            builder.Services.AddSettingsToDependencyInjection(settings);
            builder.Services.AddUsersModuleLocal(settings);

            builder.Services.AddHttpClient();
            builder.Services.AddGrpc();
            builder.AddOpenTelemetry(settings);

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
            app.MapGrpcService<UserService>();
            app.MigrateUserDatabase();
            app.MapOpenTelemetry(settings);
            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}
