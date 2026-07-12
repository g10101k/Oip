using Microsoft.AspNetCore.Builder;
using Oip.Base.Data.Extensions;
using Oip.Base.Settings;
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
    public static void UseUsersService(this WebApplication app, ISettings settings, AddingMode? addingMode = null)
    {
        var mode = addingMode ?? settings.AddingMode;

        switch (mode)
        {
            case AddingMode.Local:
                app.MigrateUserDatabase();
                break;
            case AddingMode.Service:
                app.MigrateUserDatabase();
                app.MapGrpcService<UserService>();
                break;
            case AddingMode.Remote:
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