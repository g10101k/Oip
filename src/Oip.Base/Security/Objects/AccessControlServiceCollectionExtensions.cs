using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Security.Objects;
using Oip.Base.Security.Objects.Data;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers the object access control.
/// </summary>
public static class AccessControlServiceCollectionExtensions
{
    /// <summary>
    /// Registers the securable object types of a service context. Attach the interceptor to the context with
    /// <see cref="UseOipAccessControl{TContext}" />.
    /// </summary>
    public static IServiceCollection AddOipAccessControl<TContext>(this IServiceCollection services,
        Action<AccessControlOptions<TContext>> configure)
        where TContext : DbContext, IAccessControlDbContext
    {
        var options = new AccessControlOptions<TContext>();
        configure(options);
        options.Validate();

        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddSingleton(options);
        services.AddSingleton<AccessControlRegistry<TContext>>();
        services.AddSingleton<IAccessControlRegistration>(sp => sp.GetRequiredService<AccessControlRegistry<TContext>>());
        services.AddSingleton<AccessControlSaveChangesInterceptor<TContext>>();
        services.AddScoped<ObjectAccessService<TContext>>();
        services.TryAddScoped<IObjectAccessService, ObjectAccessServiceDispatcher>();
        services.TryAddScoped<ISubjectResolver, ClaimsSubjectResolver>();
        services.TryAddScoped<ISubjectDirectory, KeycloakSubjectDirectory>();
        services.AddStartupTask<AccessControlStartupTask<TContext>>();
        return services;
    }

    /// <summary>
    /// Attaches the interceptor that registers, moves and forgets securable entities on save.
    /// </summary>
    public static DbContextOptionsBuilder UseOipAccessControl<TContext>(this DbContextOptionsBuilder builder,
        IServiceProvider serviceProvider)
        where TContext : DbContext, IAccessControlDbContext
    {
        return builder.AddInterceptors(serviceProvider.GetRequiredService<AccessControlSaveChangesInterceptor<TContext>>());
    }

    /// <summary>
    /// Keeps only the entities the caller holds the right on.
    /// </summary>
    public static IQueryable<T> WhereAccessible<T, TContext>(this IQueryable<T> query,
        ObjectAccessService<TContext> access, string right)
        where T : class, ISecurableEntity
        where TContext : DbContext, IAccessControlDbContext
    {
        return access.WhereAccessible(query, right);
    }
}
