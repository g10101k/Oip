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
        if (settings.StartupMode is StartupMode.Standalone or StartupMode.Service)
        {
            // Обновляем бд
            app.MigrateNotificationDatabase();
            // SignalR
            app.MapHub<NotificationHub>("/hubs/notification");
        }

        switch (settings.StartupMode)
        {
            case StartupMode.Standalone:
                // Регистрировать gRPC не требуется, т.к. все локально
                break;
            case StartupMode.Service:
                // gRPC
                app.MapGrpcService<NotificationService>();
                break;
            case StartupMode.Remote:
                // Регистрировать gRPC не требуется, т.к. сервис запущен в отдельном приложении
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