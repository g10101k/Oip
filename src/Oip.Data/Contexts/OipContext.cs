using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oip.Data.Entities;
using Oip.Data.EntityConfigurations;
using Oip.Settings.Enums;

namespace Oip.Data.Contexts;

/// <summary>
/// Data
/// </summary>
public class OipContext : DbContext
{
    /// <summary>
    /// Migration history table name
    /// </summary>
    public const string MigrationHistoryTableName = "MigrationHistory";

    private readonly bool _designTime;

    /// <summary>
    /// Schema
    /// </summary>
    public const string SchemaName = "oip";

    /// <summary>
    /// Modules
    /// </summary>
    public DbSet<ModuleEntity> Modules => Set<ModuleEntity>();

    /// <summary>
    /// Module Instances
    /// </summary>
    public DbSet<ModuleInstanceEntity> ModuleInstances => Set<ModuleInstanceEntity>();

    /// <summary>
    /// Module InstanceSecurities
    /// </summary>
    public DbSet<ModuleInstanceSecurityEntity> ModuleInstanceSecurities => Set<ModuleInstanceSecurityEntity>();

    /// <summary>
    /// Module Securities
    /// </summary>
    public DbSet<ModuleSecurityEntity> ModuleSecurities => Set<ModuleSecurityEntity>();

    /// <summary>
    /// Users
    /// </summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <summary>
    /// .ctor
    /// </summary>
    public OipContext(DbContextOptions<OipContext> options, bool designTime = false) : base(options)
    {
        _designTime = designTime;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            throw new InvalidOperationException("OnConfiguring error");
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ModuleEntityConfiguration(Database, _designTime));
        modelBuilder.ApplyConfiguration(new ModuleInstanceEntityConfiguration(Database, _designTime));
        modelBuilder.ApplyConfiguration(new ModuleInstanceSecurityEntityConfiguration(Database, _designTime));
        modelBuilder.ApplyConfiguration(new ModuleSecurityEntityConfiguration(Database, _designTime));

        modelBuilder.ApplyConfiguration(new UserEntityConfiguration(Database, _designTime));
    }

    /// <summary>
    /// Migrate db
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="connectionString"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void MigrateDb(XpoProvider provider, string connectionString)
    {
        var builder = new DbContextOptionsBuilder<OipContext>();

        switch (provider)
        {
            case XpoProvider.Postgres:
                builder.UseNpgsql(connectionString);
                using (var context = new PostgresMigrationContext(builder.Options, false))
                {
                    context.Database.Migrate();
                    DefaultDataInsert(context);
                }

                break;
            case XpoProvider.MSSqlServer:
                builder.UseSqlServer(connectionString);
                using (var context = new SqlServerMigrationContext(builder.Options, false))
                {
                    context.Database.Migrate();
                    DefaultDataInsert(context);
                }

                break;
            case XpoProvider.InMemoryDataStore:
                break;
            default:
                throw new InvalidOperationException("Unknown provider");
        }
    }

    private static void DefaultDataInsert(OipContext context)
    {
        AddModulesFromAssemblies(context);

        context.SaveChanges();
    }

    private static void AddModulesFromAssemblies(OipContext context)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var result = new List<Type>();
        foreach (var assembly in assemblies)
        {
            IEnumerable<Type> types = assembly.GetTypes();
            var baseCon = types.Where(x => x.BaseType?.Name.StartsWith("BaseModuleController") ?? false);
            result.AddRange(baseCon);
        }

        foreach (var type in result)
        {
            var moduleName = type.Name.Replace("Controller", string.Empty);
            RouteAttribute? attr = type.GetCustomAttribute<RouteAttribute>();
            if (attr == null) continue;
            var link = attr.Template.Replace("api", string.Empty);
            if (!context.Modules.Any(x => x.Name == moduleName))
            {
                context.Modules.Add(new ModuleEntity { Name = moduleName, RouterLink = link });
            }
        }
    }
}

internal class SqlServerMigrationContext(DbContextOptions<OipContext> options, bool designTime)
    : OipContext(options, designTime);

internal class PostgresMigrationContext(DbContextOptions<OipContext> options, bool designTime)
    : OipContext(options, designTime);