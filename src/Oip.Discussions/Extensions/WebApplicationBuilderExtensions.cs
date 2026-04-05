using Oip.Base.Settings;
using Oip.Data.Extensions;
using Oip.Discussions.Data;
using Oip.Discussions.Data.Repositories;
using Oip.Discussions.Services;
using Oip.Users.Extensions;
using Oip.Users.Repositories;

namespace Oip.Discussions.Extensions;

/// <summary>
/// Extension methods for the WebApplicationBuilder class.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the discussion service to the application.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="settings"></param>
    public static void AddDiscussionsService(this WebApplicationBuilder builder, IBaseOipModuleAppSettings settings)
    {
        builder.Services.AddOipBasedContext<DiscussionsDbContext>(settings.ConnectionString,
            DiscussionsDbContext.MigrationHistoryTableName, DiscussionsDbContext.SchemaName);
        builder.Services.AddUsersData(settings);
        builder.Services.AddScoped<AttachmentRepository>()
            .AddScoped<CommentEditHistoryRepository>()
            .AddScoped<CommentRepository>()
            .AddScoped<MentionRepository>()
            .AddScoped<ReactionRepository>()
            .AddScoped<CommentService>()
            .AddScoped<UserRepository>()
            .AddScoped<IDiscussionAttachmentStorage, LocalDiscussionAttachmentStorage>();
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
