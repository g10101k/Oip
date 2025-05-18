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
/// WebApplicationBuilder extension
/// </summary>
public static class WebApplicationBuilderExtension
{
    /// <summary>
    /// Add OIP module context
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="provider"></param>
    /// <param name="connectionString"></param>
    /// <exception cref="InvalidOperationException"></exception>
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

    public static IApplicationBuilder MigrateDatabase(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetService<OipModuleContext>()
                      ?? throw new InvalidOperationException();
        context.Database.Migrate();
        AddModulesFromAssemblies(context);
        return app;
    }

    private static void AddModulesFromAssemblies(OipModuleContext moduleContext)
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
}