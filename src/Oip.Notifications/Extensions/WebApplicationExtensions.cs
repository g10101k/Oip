using Microsoft.EntityFrameworkCore;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Hubs;
using Oip.Notifications.Services;

namespace Oip.Notifications.Extensions;

/// <summary>
/// Provides extension methods for configuring the notifications web application.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the local notifications module for the current host.
    /// </summary>
    public static void AddNotificationsModuleLocal(this WebApplication app)
    {
        app.MigrateNotificationDatabase();
        app.MapNotificationsModule();
    }

    /// <summary>
    /// Applies pending notifications database migrations.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the notifications database context cannot be resolved.</exception>
    public static void MigrateNotificationDatabase(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetService<NotificationsDbContext>()
                      ?? throw new InvalidOperationException();
        context.Database.Migrate();
    }

    /// <summary>
    /// Maps notification endpoints for the current host.
    /// </summary>
    public static void MapNotificationsModule(this WebApplication app)
    {
        app.MapGrpcService<NotificationService>();
        app.MapHub<NotificationHub>("/hubs/notification");
    }
}
