using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Oip.Base.Data.Entities;
using Oip.Base.Data.EntityConfigurations;
using Oip.Base.Data.Extensions;

namespace Oip.Base.Data.Contexts;

/// <summary>
/// OIP module context
/// </summary>
public class OipModuleContext : DbContext
{
    private readonly bool _designTime;

    /// <summary>
    /// Schema
    /// </summary>
    public const string SchemaName = "oip";

    /// <summary>
    /// Migration history table name
    /// </summary>
    public const string MigrationHistoryTableName = "__OipModuleMigrationHistory";

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
    public OipModuleContext(DbContextOptions<OipModuleContext> options, bool designTime = false) : base(options)
    {
        _designTime = designTime;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .ReplaceService<IMigrationsAssembly,
                BaseContextMigrationAssembly<OipModuleContextSqlServer, OipModuleContextPostgres>>();
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

        modelBuilder.ApplyXmlDocumentation(_designTime);
    }
}

/// <summary>
/// Represents the SQL Server database context for user-related entities.
/// </summary>
public class OipModuleContextSqlServer(DbContextOptions<OipModuleContext> options, bool designTime = true)
    : OipModuleContext(options, designTime);

/// <summary>
/// Represents the PostgreSQL database context for user-related entities.
/// </summary>
public class OipModuleContextPostgres(DbContextOptions<OipModuleContext> options, bool designTime = true)
    : OipModuleContext(options, designTime);