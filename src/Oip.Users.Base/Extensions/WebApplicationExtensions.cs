using Microsoft.AspNetCore.Builder;
using Oip.Base.Data.Extensions;
using Oip.Base.Settings;
using Oip.Settings.Enums;
using Oip.Users.Base.Contexts;
using Oip.Users.Base.Services;

namespace Oip.Users.Base.Extensions;

/// <summary>
/// Provides extension methods for configuring the users web application.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the local users module for the current host.
    /// </summary>
    public static void UseUsersService(this WebApplication app, ISettings settings)
    {
        if (settings.StartupMode is StartupMode.Standalone or StartupMode.Service)
        {
            app.MigrateUserDatabase();
        }

        switch (settings.StartupMode)
        {
            case StartupMode.Standalone:
                break;
            case StartupMode.Service:
                app.MapGrpcService<UserService>();
                break;
            case StartupMode.Remote:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Applies pending users database migrations.
    /// </summary>
    private static void MigrateUserDatabase(this WebApplication app)
    {
        app.MigrateDatabase<UserContext>();
    }
}
