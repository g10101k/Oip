using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Settings;
using Oip.Notifications.Base.Data.Contexts;
using Oip.Notifications.Base.Hubs;
using Oip.Notifications.Base.Services;

namespace Oip.Notifications.Base.Extensions;

/// <summary>
/// Provides extension methods for configuring the notifications web application.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the local notifications module for the current host.
    /// </summary>
    public static void UseNotificationsService(this WebApplication app, ISettings settings)
    {
        if (settings.ServiceAddingMode is AddingMode.Local or AddingMode.Service)
        {
            // Update the database.
            app.MigrateNotificationDatabase();
            // SignalR
            app.MapHub<NotificationHub>("/hubs/notification");
        }

        switch (settings.ServiceAddingMode)
        {
            case AddingMode.Local:
                // No need to register gRPC because everything is local.
                break;
            case AddingMode.Service:
                // Register gRPC.
                app.MapGrpcService<NotificationService>();
                break;
            case AddingMode.Remote:
                // No need to register gRPC because the service runs in a separate application.
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Applies pending notifications database migrations.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the notifications database context cannot be resolved.</exception>
    private static void MigrateNotificationDatabase(this WebApplication app)
    {
        using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetService<NotificationsDbContext>()
                      ?? throw new InvalidOperationException();
        context.Database.Migrate();
    }
}
