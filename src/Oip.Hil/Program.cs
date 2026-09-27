using NLog;
using NLog.Web;
using Oip.Applications.Base.Controllers;
using Oip.Applications.Base.Extensions;
using Oip.Base.Data.Extensions;
using Oip.Base.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Security.DefaultSecrets;
using Oip.Base.Services;
using Oip.Base.Settings;
using Oip.Discussions.Base.Controllers;
using Oip.Discussions.Base.Extensions;
using Oip.Hil.Base.Controllers;
using Oip.Hil.Base.Workflows;
using Oip.Hil.Controllers;
using Oip.Hil.Settings;
using Oip.Hil.Workflows;
using Oip.Notifications.Base.Controllers;
using Oip.Notifications.Base.Extensions;
using Oip.Users.Base.Controllers;
using Oip.Users.Base.Extensions;

namespace Oip.Hil;

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
            builder.Services.AddOipModuleContext(settings.ConnectionString);
            builder.Services.AddDefaultHealthChecks();
            builder.Services.AddDefaultAuthentication(settings);
            builder.Services.AddOpenApi(settings);
            builder.Services.AddWebClientGenerationStartupTask(settings);
            builder.Services.AddDefaultSecretsValidation();
            builder.Services.AddStartupRunner();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<ClaimService>();
            builder.Services.AddCors(settings);
            builder.Services.AddDataProtection(settings);
            builder.Services.AddForwardedHeaders(settings);
            builder.Services.AddOipLocalization();
            builder.Services.AddOpenTelemetry(settings);
            builder.Services.AddControllersAndView();
            builder.Services
                .AddApplicationControllers()
                .AddController<WorkflowActivityModuleController>()
                .AddController<WorkflowDemoController>()
                .AddController<WorkflowStepController>();
            // Controllers are registered explicitly, so the controllers of the services hosted in the application
            // have to be registered too; in Remote mode they are served by the services themselves.
            if (settings.ServiceAddingMode == AddingMode.Local)
            {
                builder.Services
                    .AddController<ApplicationsController>()
                    .AddController<UsersController>()
                    .AddController<UserProfileController>()
                    .AddController<KeycloakEventsController>()
                    .AddController<DiscussionController>()
                    .AddController<NotificationController>();
            }

            builder.Services.AddUserService(settings);
            builder.Services.AddDiscussionsService(settings);
            builder.Services.AddNotificationsService(settings);
            builder.Services.AddApplicationsService(settings);
            builder.Services.AddOipWorkflows(settings.Temporal, workflows => workflows
                .AddWorkflow<HelloWorldWorkflow>()
                .AddWorkflow<UserTaskDemoWorkflow>());

            var app = builder.Build();
            app.UseOipSpa(settings);
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
            app.MapOpenTelemetry(settings);
            app.UseApplicationsService(settings);
            app.UseUsersService(settings);
            app.UseDiscussionsService(settings);
            app.UseNotificationsService(settings);
            app.MigrateOipModuleDatabase();

            app.Run();
        }
        catch (OperationCanceledException)
        {
            logger.Info("Application execution cancelled, see logs for details.");
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}
