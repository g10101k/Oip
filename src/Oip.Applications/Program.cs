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
            builder.Services.AddSingleton<IBaseOipModuleAppSettings>(settings);
            builder.Services.AddSettingsToDependencyInjection(settings);
            builder.Services.AddApplicationsModuleLocal(settings);
            builder.AddDefaultHealthChecks();
            builder.AddDefaultAuthentication(settings);
            builder.AddOpenApi(settings);
            builder.Services.AddGrpc();
            builder.Services.GenerateWebClientStartupTask(settings);
            builder.Services.AddStartupRunner();
            builder.Services.AddCors();
            builder.AddOipForwardedHeaders(settings);
            builder.AddControllersAndView();
            builder.Services
                .AddController<ApplicationsController>()
                .AddController<SecurityController>();
            builder.AddLocalization();
            builder.AddOpenTelemetry(settings);

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
            app.MapGrpcService<GrpcApplicationRegistryService>();
            app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
            app.MapOpenApi(settings);
            app.MapOpenTelemetry(settings);

            app.MigrateDatabase<ApplicationRegistryDbContext>();

            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}
