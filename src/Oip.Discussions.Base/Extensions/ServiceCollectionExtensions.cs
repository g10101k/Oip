using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minio;
using Oip.Base.Extensions;
using Oip.Base.Settings;
using Oip.Discussions.Base.Controllers;
using Oip.Discussions.Base.Data;
using Oip.Discussions.Base.Data.Repositories;
using Oip.Discussions.Base.Services;
using Oip.Discussions.Base.Settings;

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
        AddingMode? addingMode = null)
    {
        var mode = addingMode ?? settings.ServiceAddingMode;
        
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
        services.AddDiscussionAttachmentStorage();

        return services;
    }

    private static IServiceCollection AddDiscussionAttachmentStorage(this IServiceCollection services)
    {
        services.TryAddSingleton<DiscussionAttachmentMinioClient>(sp =>
        {
            var settings = sp.GetRequiredService<DiscussionAttachmentStorageSettings>();
            var client = new MinioClient()
                .WithEndpoint(settings.Endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey);

            if (settings.UseSsl)
            {
                client = client.WithSSL();
            }

            return new DiscussionAttachmentMinioClient(client.Build());
        });
        services.TryAddScoped<IDiscussionAttachmentStorage, MinioDiscussionAttachmentStorage>();
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
