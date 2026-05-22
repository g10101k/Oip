using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Oip.Applications.Data.EntityConfigurations;
using Oip.Data.Contexts;
using Oip.Data.Extensions;

namespace Oip.Applications.Data;

/// <summary>
/// Database context for the application registry.
/// </summary>
public class ApplicationRegistryDbContext: DbContext
{
    private readonly bool _designTime;

    /// <summary>
    /// Schema name for application registry entities.
    /// </summary>
    public const string SchemaName = "applications";

    /// <summary>
    /// Table name for tracking Entity Framework Core migrations.
    /// </summary>
    public const string MigrationHistoryTableName = "__EFMigrationHistory";

    /// <summary>
    /// Application registry items.
    /// </summary>
    public DbSet<ApplicationRegistryItemEntity> ApplicationRegistryItems => Set<ApplicationRegistryItemEntity>();

    public ApplicationRegistryDbContext(DbContextOptions<ApplicationRegistryDbContext> options, bool designTime = false) : base(options)
    {
        _designTime = designTime;
    }
    
    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .ReplaceService<IMigrationsAssembly,
                BaseContextMigrationAssembly<ApplicationRegistryDbContextSqlServer,
                    ApplicationRegistryDbContextPostgres>>();

        if (!optionsBuilder.IsConfigured)
            throw new InvalidOperationException("OnConfiguring error");
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ApplicationRegistryItemEntityConfiguration(Database));
        modelBuilder.ApplyXmlDocumentation(_designTime);
    }
}

/// <summary>
/// SQL Server application registry database context.
/// </summary>
public class ApplicationRegistryDbContextSqlServer(
    DbContextOptions<ApplicationRegistryDbContext> options,
    bool designTime = true)
    : ApplicationRegistryDbContext(options, designTime);

/// <summary>
/// PostgreSQL application registry database context.
/// </summary>
public class ApplicationRegistryDbContextPostgres(
    DbContextOptions<ApplicationRegistryDbContext> options,
    bool designTime = true)
    : ApplicationRegistryDbContext(options, designTime);