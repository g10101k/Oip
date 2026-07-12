using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oip.Applications.Base.Data.Contexts;
using Oip.Applications.Base.Services;
using Oip.Base.Settings;
using Oip.Settings.Enums;

namespace Oip.Applications.Base.Extensions;

/// <summary>
/// Provides extension methods for configuring the applications web application.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the applications module for the current host.
    /// </summary>
    public static void UseApplicationsService(this WebApplication app, ISettings settings)
    {
        if (settings.StartupMode is StartupMode.Standalone or StartupMode.Service)
        {
            app.MigrateApplicationsDatabase();
        }

        switch (settings.StartupMode)
        {
            case StartupMode.Standalone:
                break;
            case StartupMode.Service:
                app.MapGrpcService<GrpcApplicationRegistryService>();
                break;
            case StartupMode.Remote:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Applies pending application registry database migrations.
    /// </summary>
    private static void MigrateApplicationsDatabase(this WebApplication app)
    {
        using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetService<ApplicationRegistryDbContext>()
                      ?? throw new InvalidOperationException();
        context.Database.Migrate();
    }
}
