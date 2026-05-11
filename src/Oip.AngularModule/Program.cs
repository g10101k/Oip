using NLog;
using NLog.Web;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Oip.Base.Services;
using Oip.Base.StartupTasks;
using Oip.Data.Extensions;
using Oip.Discussions.Extensions;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Extensions;
using Oip.Settings;
using Oip.Users.Notifications;
using Oip.Users.Services;
using Oip.Users.Extensions;
using GrpcUserServiceImpl = Oip.Users.Services.UserService;

namespace Oip;

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
            builder.AddControllersAndView();
            builder.AddLocalization();
            builder.AddOpenTelemetry(settings);

            if (settings.IsStandalone)
            {
                builder.Services.AddUsersModuleLocal(settings);
                builder.Services.AddScoped<GrpcUserServiceImpl>();
                builder.Services.AddScoped<UserSyncService>();
                builder.Services.AddSingleton<INotificationPublisher, NoOpNotificationPublisher>();

                builder.Services.AddDiscussionsModuleLocal(settings);

                builder.Services.AddNotificationsModuleLocal(settings);
                builder.Services.AddDataProtection<NotificationsDbContext>();
                builder.Services.AddSignalR();
                builder.Services.AddGrpc().AddJsonTranscoding();
                builder.Services.AddGrpcSwagger();
                builder.Services.AddSingleton<CryptService>();
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
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors(options => options.AllowAnyOrigin());
            app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
            app.MapGet("/manifest.json", (HttpRequest request) =>
            {
                var origin = $"{request.Scheme}://{request.Host}";
                return Results.Json(new
                {
                    key = "oip-angular-module",
                    name = "OIP Angular Module",
                    version = "1.0.0",
                    routePath = "extensions/oip-angular-module",
                    loadType = "moduleFederation",
                    remoteEntryUrl = $"{origin}/remoteEntry.js",
                    exposedModule = "./ExternalModuleExampleModule",
                    componentName = "ExternalModuleExampleModuleComponent",
                    apiBaseUrl = $"{origin}/api",
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
                app.MigrateUserDatabase();
                app.AddDiscussions(settings);
                app.MigrateNotificationDatabase();
                app.MapNotificationsModule();
            }

            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}
