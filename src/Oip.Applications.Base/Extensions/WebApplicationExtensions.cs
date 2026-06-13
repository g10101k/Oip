using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oip.Applications.Base.Data;

namespace Oip.Applications.Base.Extensions;

/// <summary>
/// Provides extension methods for configuring the users web application.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the local users module for the current host.
    /// </summary>
    public static void UseOipApplications(this WebApplication app)
    {
        app.MigrateOipApplicationsDatabase();
    }

    private static void MigrateOipApplicationsDatabase(this WebApplication app)
    {
        using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetService<ApplicationRegistryDbContext>()
                      ?? throw new InvalidOperationException();
        context.Database.Migrate();
    }
}
