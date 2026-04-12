using Oip.Base.Settings;
using Oip.Data.Extensions;
using Oip.Discussions.Data;
using Oip.Discussions.Data.Repositories;
using Oip.Discussions.Services;

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
        if (settings.IsStandalone)
        {
            builder.Services.AddDiscussionsModuleLocal(settings);
        }
        else
        {
            builder.Services.AddDiscussionsModuleRemote(settings);
        }
    }

    /// <summary>
    /// Registers the discussions module for local standalone composition.
    /// </summary>
    public static void AddDiscussionsModuleLocal(this IServiceCollection services, IBaseOipModuleAppSettings settings)
    {
        AddDiscussionsModuleCore(services, settings);
    }

    /// <summary>
    /// Registers the discussions module for distributed mode.
    /// </summary>
    public static void AddDiscussionsModuleRemote(this IServiceCollection services,
        IBaseOipModuleAppSettings settings)
    {
        AddDiscussionsModuleCore(services, settings);
    }

    private static void AddDiscussionsModuleCore(IServiceCollection services, IBaseOipModuleAppSettings settings)
    {
        services.AddOipBasedContext<DiscussionsDbContext>(settings.ConnectionString,
            DiscussionsDbContext.MigrationHistoryTableName, DiscussionsDbContext.SchemaName);
        services.AddScoped<AttachmentRepository>()
            .AddScoped<CommentEditHistoryRepository>()
            .AddScoped<CommentRepository>()
            .AddScoped<MentionRepository>()
            .AddScoped<ReactionRepository>()
            .AddScoped<CommentService>()
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