using Microsoft.EntityFrameworkCore;
using Oip.Users.Contexts;

namespace Oip.Users.Extensions;

/// <summary>
/// Provides extension methods for configuring the users web application.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the local users module for the current host.
    /// </summary>
    public static void AddUserModuleLocal(this WebApplication app)
    {
        app.MigrateUserDatabase();
    }

    /// <summary>
    /// Applies pending users database migrations.
    /// </summary>
    public static void MigrateUserDatabase(this WebApplication app)
    {
        using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetService<UserContext>()
                      ?? throw new InvalidOperationException();
        context.Database.Migrate();
    }
}
