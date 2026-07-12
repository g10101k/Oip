using NLog;
using NLog.Web;
using Oip.Applications.Base.Controllers;
using Oip.Applications.Base.Data;
using Oip.Applications.Base.Data.Contexts;
using Oip.Applications.Base.Extensions;
using Oip.Applications.Base.Services;
using Oip.Applications.Base.Settings;
using Oip.Base.Controllers;
using Oip.Base.Data.Extensions;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;

namespace Oip.Applications;

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
            builder.Services
                .AddSingleton<ISettings>(settings)
                .AddSettingsToDependencyInjection(settings)
                .AddApplicationsService(settings)
                .AddDefaultHealthChecks()
                .AddDefaultAuthentication(settings)
                .AddOpenApi(settings)
                .AddStartupRunner()
                .AddCors(settings)
                .AddForwardedHeaders(settings)
                .AddControllersAndView()
                .AddController<ApplicationsController>()
                .AddController<SecurityController>()
                .AddOipLocalization()
                .AddOpenTelemetry(settings);

            var app = builder.Build();
            app.UseOipForwardedHeaders();
            app.AddRequestLocalization();
            app.AddExceptionHandler();
            app.MapDefaultEndpoints();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseOipCsrfProtection();
            app.UseAuthorization();
            app.UseCors(options => options.AllowAnyOrigin());
            app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
            app.MapOpenApi(settings);
            app.MapOpenTelemetry(settings);

            app.UseApplicationsService(settings);

            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}
