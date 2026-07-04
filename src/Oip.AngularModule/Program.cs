using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Oip.AngularModule.Controllers;
using Oip.AngularModule.Settings;
using Oip.Base.Controllers;
using Oip.Base.Data.Extensions;
using Oip.Base.Extensions;
using Oip.Base.Providers;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Base.StartupTasks;
using Oip.Discussions.Base.Extensions;
using Oip.Notifications.Base.Extensions;
using Oip.Users.Base.Extensions;

namespace Oip.AngularModule;

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
            builder.Services.AddOipModuleContext(settings.ConnectionString);
            builder.AddDefaultHealthChecks();
            builder.AddDefaultAuthentication(settings);
            builder.AddOpenApi(settings);
            builder.Services.AddStartupTask<SwaggerGenerateWebClientStartupTask>();
            builder.Services.AddStartupRunner();
            builder.Services.AddHttpClient();
            builder.Services.AddCors();
            builder.Services.AddOipCors(settings);
            builder.Services.AddOipDataProtection(settings);
            builder.AddControllersAndView();
            builder.Services
                .AddController<ExternalModuleExampleModuleController>()
                .AddController<FolderModuleController>()
                .AddController<SecurityController>();
            builder.AddLocalization();
            builder.AddOpenTelemetry(settings);

            if (settings.IsStandalone)
            {
                builder.Services.AddUsersModuleLocal(settings);
                builder.Services.AddDiscussionsModuleLocal(settings);
                builder.Services.AddNotificationsModuleLocal(settings);
                builder.Services.AddSignalR();
                builder.Services.AddGrpc();
            }
            else
            {
                builder.Services.AddUsersModuleRemote(settings);
            }

            var app = builder.Build();


            app.AddRequestLocalization();
            app.AddExceptionHandler();
            app.MapDefaultEndpoints();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseOipCsrfProtection();
            app.UseAuthorization();
            app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
            app.MapGet("/manifest.json", (HttpRequest request) =>
            {
                var origin = $"{request.Scheme}://{request.Host}";
                return Results.Json(new
                {
                    key = "external-module-example-module",
                    name = "OIP Angular Module",
                    version = "1.0.0",
                    loadType = "moduleFederation",
                    remoteEntryUrl = $"{origin}/remoteEntry.js",
                    exposedModule = "./ExternalModuleExampleModule",
                    componentName = "ExternalModuleExampleModuleComponent",
                    apiBaseUrl = origin,
                    icon = "pi pi-th-large",
                    description = "Angular Module Federation extension loaded by the main OIP application."
                });
            });
            app.MapOpenApi(settings);
            app.MapFallbackToFile("index.html");
            app.MapOpenTelemetry(settings);

            app.MigrateOipModuleDatabase();

            if (settings.IsStandalone)
            {
                app.AddUserModuleLocal();
                app.AddDiscussionsModuleLocal();
                app.AddNotificationsModuleLocal();
            }

            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}
