using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Entities;
using Oip.Settings.Enums;

namespace Oip.Base.Extensions;

/// <summary>
/// Provides extension methods for configuring and initializing the OIP module context
/// within an ASP.NET Core <see cref="WebApplicationBuilder"/> and <see cref="IApplicationBuilder"/>.
/// </summary>
public static class WebApplicationBuilderExtension
{
    /// <summary>
    /// Adds the <see cref="OipModuleContext"/> to the service collection based on the specified database provider.
    /// </summary>
    /// <param name="builder">The current <see cref="WebApplicationBuilder"/>.</param>
    /// <param name="provider">The database provider to use (e.g., Postgres, MSSqlServer).</param>
    /// <param name="connectionString">The connection string to the database.</param>
    /// <exception cref="InvalidOperationException">Thrown if the specified provider is not supported.</exception>
    public static void AddOipModuleContext(this WebApplicationBuilder builder, XpoProvider provider,
        string connectionString)
    {
        switch (provider)
        {
            case XpoProvider.Postgres:
                builder.Services.AddDbContext<OipModuleContext>(option =>
                {
                    option.UseNpgsql(connectionString, x =>
                    {
                        x.MigrationsAssembly("Oip.Base.Data.Postgres");
                        x.MigrationsHistoryTable(OipModuleContext.MigrationHistoryTableName,
                            OipModuleContext.SchemaName);
                    });
                });
                break;
            case XpoProvider.MSSqlServer:
                builder.Services.AddDbContext<OipModuleContext>(option =>
                {
                    option.UseSqlServer(connectionString, x =>
                    {
                        x.MigrationsAssembly("Oip.Base.Data.SqlServer");
                        x.MigrationsHistoryTable(OipModuleContext.MigrationHistoryTableName,
                            OipModuleContext.SchemaName);
                    });
                });
                break;
            case XpoProvider.InMemoryDataStore:
                throw new InvalidOperationException("Provider InMemoryDataStore is not supported");
        }
    }

    /// <summary>
    /// Applies any pending migrations for the OIP module context and registers discovered modules from loaded assemblies.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance, to support method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="OipModuleContext"/> cannot be resolved.</exception>
    public static IApplicationBuilder MigrateDatabase(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetService<OipModuleContext>()
                      ?? throw new InvalidOperationException();
        context.Database.Migrate();
        AddModulesFromAssemblies(context);
        return app;
    }

    /// <summary>
    /// Scans all currently loaded assemblies and adds module metadata to the database
    /// if the module is not already registered.
    /// </summary>
    /// <param name="moduleContext">The database context used to persist module information.</param>
    private static void AddModulesFromAssemblies(OipModuleContext moduleContext)
    {
        var result = GetAllLoadedModules();

        foreach (var type in result)
        {
            var moduleName = type.Name.Replace("Controller", string.Empty);
            RouteAttribute? attr = type.GetCustomAttribute<RouteAttribute>();
            if (attr == null) continue;
            var link = attr.Template.Replace("api", string.Empty);
            if (!moduleContext.Modules.Any(x => x.Name == moduleName))
            {
                moduleContext.Modules.Add(new ModuleEntity { Name = moduleName, RouterLink = link });
            }
        }

        moduleContext.SaveChanges();
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
