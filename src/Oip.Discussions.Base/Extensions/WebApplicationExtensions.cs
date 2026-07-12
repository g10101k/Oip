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
    /// Configures the discussions module for the current host.
    /// </summary>
    public static void UseDiscussionsService(this WebApplication app, ISettings settings)
    {
        if (settings.ServiceAddingMode is AddingMode.Local or AddingMode.Service)
        {
            app.MigrateDatabase<DiscussionsDbContext>();
        }

        switch (settings.ServiceAddingMode)
        {
            case AddingMode.Local:
            case AddingMode.Service:
            case AddingMode.Remote:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
