using Microsoft.AspNetCore.Builder;
using Oip.Data.Extensions;
using Oip.Users.Base.Contexts;

namespace Oip.Users.Base.Extensions;

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
        app.MigrateDatabase<UserContext>();
    }
}
