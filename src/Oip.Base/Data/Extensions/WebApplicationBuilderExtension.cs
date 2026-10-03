using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oip.Base.Controllers;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Entities;

namespace Oip.Base.Data.Extensions;

/// <summary>
/// Provides extension methods for configuring and initializing the OIP module context
/// within an ASP.NET Core <see cref="WebApplicationBuilder"/> and <see cref="IApplicationBuilder"/>.
/// </summary>
public static class WebApplicationBuilderExtension
{
    /// <summary>
    /// Applies any pending migrations for the OIP module context and registers discovered modules from loaded assemblies.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance, to support method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="OipModuleContext"/> cannot be resolved.</exception>
    public static IApplicationBuilder MigrateOipModuleDatabase(this IApplicationBuilder app)
    {
        using var context = app.MigrateDatabaseInternal<OipModuleContext>();
        using var scope = app.ApplicationServices.CreateScope();
        AddModulesFromAssemblies(context, scope.ServiceProvider);
        return app;
    }

    /// <summary>
    /// Applies any pending migrations for the specified database context.
    /// </summary>
    /// <typeparam name="T">The type of the database context to migrate.</typeparam>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance, to support method chaining.</returns>
    public static IApplicationBuilder MigrateDatabase<T>(this IApplicationBuilder app) where T : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<T>();
        context.Database.Migrate();
        return app;
    }

    /// <summary>
    /// Applies any pending migrations for the specified database context.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <typeparam name="T">The type of the <see cref="DbContext"/> to migrate.</typeparam>
    /// <returns>The migrated <see cref="DbContext"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified <see cref="DbContext"/> cannot be resolved.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T MigrateDatabaseInternal<T>(this IApplicationBuilder app) where T : DbContext
    {
        var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetService<T>() ??
                      throw new InvalidOperationException($"Could not find {typeof(T)}.");
        context.Database.Migrate();
        return context;
    }

    /// <summary>
    /// Scans all currently loaded assemblies and adds module metadata to the database
    /// if the module is not already registered.
    /// </summary>
    /// <param name="moduleContext">The database context used to persist module information.</param>
    /// <param name="serviceProvider">The service provider used to create module controllers to read their icons.</param>
    private static void AddModulesFromAssemblies(OipModuleContext moduleContext, IServiceProvider serviceProvider)
    {
        var result = GetAllLoadedModules();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(WebApplicationBuilderExtension));

        foreach (var type in result)
        {
            var moduleName = type.Name.Replace("Controller", string.Empty);
            var attr = type.GetCustomAttribute<RouteAttribute>();
            if (attr == null) continue;
            var link = attr.Template.Replace("api", string.Empty);
            var iconResolved = TryGetModuleIcon(type, serviceProvider, logger, out var icon);
            var module = moduleContext.Modules.FirstOrDefault(m => m.Name == moduleName);
            if (module is null)
            {
                moduleContext.Modules.Add(new ModuleEntity { Name = moduleName, RouterLink = link, Icon = icon });
            }
            else
            {
                module.RouterLink = link;
                if (iconResolved)
                    module.Icon = icon;
            }
        }

        moduleContext.SaveChanges();
    }

    /// <summary>
    /// Creates the module controller and reads its <c>Icon</c> property.
    /// </summary>
    /// <param name="type">The module controller type.</param>
    /// <param name="serviceProvider">The service provider used to resolve controller dependencies.</param>
    /// <param name="logger">The logger used to report controllers that cannot be created.</param>
    /// <param name="icon">The module icon, or <c>null</c> when the module does not declare one.</param>
    /// <returns><c>true</c> when the icon was read; <c>false</c> when the controller could not be created.</returns>
    private static bool TryGetModuleIcon(Type type, IServiceProvider serviceProvider, ILogger logger, out string? icon)
    {
        icon = null;
        if (type.IsAbstract || type.ContainsGenericParameters)
            return false;

        try
        {
            var controller = ActivatorUtilities.CreateInstance(serviceProvider, type);
            icon = type.GetProperty(nameof(BaseModuleController<object>.Icon))?.GetValue(controller) as string;
            (controller as IDisposable)?.Dispose();
            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Could not read the icon of module {ModuleType}", type.FullName);
            return false;
        }
    }

    /// <summary>
    /// Scans all currently loaded assemblies in the application domain and returns a list of types
    /// that inherit from either <c>BaseModuleController</c> or <c>BaseDbMigrationController</c>.
    /// </summary>
    /// <returns>
    /// A list of <see cref="Type"/> objects representing classes that are derived from
    /// <c>BaseModuleController</c> or <c>BaseDbMigrationController</c>.
    /// </returns>
    /// <remarks>
    /// This method is typically used to discover and register application modules or database migration controllers
    /// at runtime by analyzing the type hierarchy in loaded assemblies.
    /// </remarks>
    private static List<Type> GetAllLoadedModules()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var result = new List<Type>();
        foreach (var assembly in assemblies)
        {
            try
            {
                IEnumerable<Type> types = assembly.GetTypes();
                var baseCon = types.Where(x =>
                    (x.BaseType?.Name.StartsWith("BaseModuleController") ?? false)
                    || (x.BaseType?.Name.StartsWith("BaseDbMigrationController") ?? false)
                );
                result.AddRange(baseCon);
            }
            catch (ReflectionTypeLoadException e)
            {
                Console.WriteLine(e);
            }
        }

        return result;
    }

    /// <summary>
    /// Asynchronously retrieves a list of all loaded module types from the currently loaded assemblies.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of
    /// <see cref="Type"/> objects that inherit from either <c>BaseModuleController</c>
    /// or <c>BaseDbMigrationController</c>.
    /// </returns>
    public static Task<List<Type>> GetAllLoadedModulesAsync()
    {
        return Task.Run(GetAllLoadedModules);
    }
}