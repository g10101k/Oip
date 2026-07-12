using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Extensions;
using Oip.Base.Settings;
using Oip.Discussions.Base.Controllers;
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
    /// Registers the discussions module services.
    /// </summary>
    public static IServiceCollection AddDiscussionsService(this IServiceCollection services, ISettings settings,
        AddingMode? startupMode = null)
    {
        var mode = startupMode ?? settings.AddingMode;
        
        switch (mode)
        {
            case AddingMode.Local:
                services.AddDiscussionData(settings);
                services.AddLocalServices();
                break;
            case AddingMode.Service:
                services.AddDiscussionData(settings);
                services.AddLocalServices();
                services
                    .AddBaseServiceControllers()
                    .AddController<DiscussionController>();
                break;
            case AddingMode.Remote:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return services;
    }

    /// <summary>
    /// Registers local discussions business services.
    /// </summary>
    public static IServiceCollection AddLocalServices(this IServiceCollection services)
    {
        services.AddScoped<CommentService>();
        services.AddScoped<IDiscussionAttachmentStorage, LocalDiscussionAttachmentStorage>();

        return services;
    }

    /// <summary>
    /// Adds discussions data services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddDiscussionData(this IServiceCollection services, ISettings settings)
    {
        services.AddOipBasedContext<DiscussionsDbContext>(
            settings.ConnectionString,
            DiscussionsDbContext.MigrationHistoryTableName,
            DiscussionsDbContext.SchemaName);

        services.AddScoped<AttachmentRepository>();
        services.AddScoped<CommentEditHistoryRepository>();
        services.AddScoped<CommentRepository>();
        services.AddScoped<MentionRepository>();
        services.AddScoped<ReactionRepository>();

        return services;
    }
}