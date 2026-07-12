using NLog;
using NLog.Web;
using Oip.Applications.Base.Extensions;
using Oip.Base.Controllers;
using Oip.Base.Extensions;
using Oip.Base.Settings;
using Oip.Notifications.Base.Extensions;
using Oip.Users.Base.Controllers;
using Oip.Users.Base.Extensions;
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

            if (settings.ServiceAddingMode != AddingMode.Service)
            {
                logger.Warn("Oip.Users must be configured with ServiceAddingMode.Service.");
                return;
            }

            var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgramArguments);
            builder.AddNlog();
            builder.Services.AddDefaultHealthChecks();
            builder.Services.AddDefaultAuthentication(settings);
            builder.Services.AddOpenApi(settings);
            builder.Services.AddSingleton<ISettings>(settings);
            builder.Services.AddSettingsToDependencyInjection(settings);
            builder.Services.AddApplicationsService(settings);
            builder.Services.AddSingleton(settings);
            builder.Services.AddCors(settings);
            builder.Services.AddForwardedHeaders(settings);
            builder.Services.AddControllersAndView();
            builder.Services
                .AddController<ProxySettingsController>()
                .AddController<SecurityController>()
                .AddController<UserProfileController>()
                .AddController<KeycloakEventsController>()
                .AddController<UsersController>();
            builder.Services.AddOipLocalization();
            builder.Services.AddSettingsToDependencyInjection(settings);
            builder.Services.AddUserService(settings);
            builder.Services.AddNotificationsService(settings, AddingMode.Remote);
            builder.Services.AddHttpClient();
            builder.Services.AddGrpc();
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
            app.MapFallbackToFile("index.html");
            app.MapOpenApi(settings);
            app.UseUsersService(settings);
            app.MapOpenTelemetry(settings);
            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}
