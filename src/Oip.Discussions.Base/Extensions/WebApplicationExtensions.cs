using Microsoft.AspNetCore.Builder;
using Oip.Base.Data.Extensions;
using Oip.Base.Settings;
using Oip.Discussions.Base.Data;

namespace Oip.Discussions.Base.Extensions;

/// <summary>
/// Provides extension methods for configuring the discussions web application.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the local discussions module for the current host.
    /// </summary>
    public static void AddDiscussionsModuleLocal(this WebApplication app)
    {
        app.MigrateDatabase<DiscussionsDbContext>();
    }

    /// <summary>
    /// Adds the discussions service to the application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="settings">The application settings.</param>
    public static void AddDiscussions(this WebApplication app, IBaseOipModuleAppSettings settings)
    {
        app.MigrateDatabase<DiscussionsDbContext>();
    }
}
