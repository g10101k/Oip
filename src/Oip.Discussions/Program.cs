using NLog;
using NLog.Web;
using Oip.Applications.Base.Extensions;
using Oip.Base.Controllers;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Discussions.Base.Controllers;
using Oip.Discussions.Base.Extensions;
using Oip.Discussions.Base.Settings;
using Oip.Users.Base.Extensions;

namespace Oip.Discussions;

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
                logger.Warn("Oip.Discussions must be configured with ServiceAddingMode.Service.");
                return;
            }

            var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgramArguments);

            builder.AddNlog();
            builder.Services.AddSingleton<ISettings>(settings);
            builder.Services.AddSettingsToDependencyInjection(settings);
            builder.Services.AddOipModuleContext(settings.ConnectionString);
            builder.Services.AddDefaultHealthChecks();
            builder.Services.AddDefaultAuthentication(settings);
            builder.Services.AddOpenApi(settings);
            builder.Services.AddStartupRunner()
                .AddCors(settings)
                .AddApplicationsService(settings)
                .AddUserService(settings)
                .AddDiscussionsService(settings)
                .AddForwardedHeaders(settings)
                .AddControllersAndView();
            builder.Services
                .AddController<DiscussionController>()
                .AddController<FolderModuleController>()
                .AddController<IframeModuleController>()
                .AddController<MenuController>()
                .AddController<ModuleController>()
                .AddController<ProxySettingsController>()
                .AddController<SecurityController>();
            builder.Services.AddOipLocalization();

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

            app.UseDiscussionsService(settings);

            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}
