using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Settings;
using Oip.Discussions.Base.Data;
using Oip.Discussions.Base.Data.Repositories;
using Oip.Discussions.Base.Services;

namespace Oip.Discussions.Base.Extensions;

/// <summary>
/// Provides extension methods for configuring discussions module services.
/// </summary>
public static class ServiceCollectionExtensions
{
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
}
