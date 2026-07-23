using NLog;
using NLog.Web;
using Oip.Applications.Base.Extensions;
using Oip.Base.Controllers;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Services;
using Oip.Base.Settings;
using Oip.Rtds.Controllers;
using Oip.Rtds.Data;
using Oip.Rtds.Data.Extensions;
using Oip.Rtds.HostedService;
using Oip.Rtds.Services;
using Oip.Rtds.Settings;

namespace Oip.Rtds;

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
            builder.Services.AddOipModuleContext(settings.NormalizedConnectionString);
            builder.Services.AddDefaultHealthChecks();
            builder.Services.AddDefaultAuthentication(settings);
            builder.Services.AddOpenApi(settings);
            builder.Services.AddApplicationsService(settings);
            builder.Services.AddWebClientGenerationStartupTask(settings);
            builder.Services.AddStartupRunner();
            builder.Services.AddSingleton(settings);
            builder.Services.AddScoped<ClaimService>();
            builder.Services.AddCors();
            builder.Services.AddForwardedHeaders(settings);
            builder.Services.AddControllersAndView();
            builder.Services
                .AddController<FolderModuleController>()
                .AddController<IframeModuleController>()
                .AddController<MenuController>()
                .AddController<ModuleController>()
                .AddController<ProxySettingsController>()
                .AddController<SecurityController>()
                .AddController<RtdsMetaDataContextMigrationModuleController>()
                .AddController<TagManagementModuleController>();
            builder.Services.AddOipLocalization();
            builder.Services.AddGrpc();
            builder.Services.AddSingleton<RtdsService>();
            builder.Services.AddSingleton<IRtdsAppSettings>(AppSettings.Instance);
            builder.Services.AddScoped<TagService>();
            builder.Services.AddRtdsData(settings);
            builder.Services.AddHostedService<RtdsHostedService>();
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
            app.UseCors(options => options.AllowAnyOrigin());
            app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
            app.MapOpenApi(settings);
            app.MapFallbackToFile("index.html");
            app.MapGrpcService<RtdsService>();
            app.MapOpenTelemetry(settings);
            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}
