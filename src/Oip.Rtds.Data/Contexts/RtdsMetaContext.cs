using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Extensions;
using Oip.Rtds.Data.Entities;
using Oip.Rtds.Data.EntityConfigurations;

namespace Oip.Rtds.Data.Contexts;

/// <summary>
/// Represents the database context for RTDS (Real-Time Data System) metadata.
/// This context manages the storage and retrieval of tag configurations and related metadata.
/// </summary>
/// <remarks>
/// This context is configured to work with the RTDS schema in the database.
/// </remarks>
public class RtdsMetaContext : DbContext
{
    private readonly bool _designTime;

    /// <summary>
    /// The name of the database schema used for RTDS metadata
    /// </summary>
    public const string SchemaName = "rtds";

    /// <summary>
    /// The name of the table used to store EF Core migration history
    /// </summary>
    public const string MigrationHistoryTableName = "__MigrationHistory";

    /// <summary>
    /// Gets the DbSet for managing tag entities
    /// </summary>
    public DbSet<TagEntity> Tags => Set<TagEntity>();

    /// <summary>
    /// Represents the interfaces associated with RTDS metadata.
    /// </summary>
    public DbSet<InterfaceEntity> Interfaces => Set<InterfaceEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="RtdsMetaContext"/> class
    /// </summary>
    /// <param name="options">The options to be used by this context</param>
    /// <param name="designTime">
    /// Flag indicating whether the context is being created at design time (e.g. for migrations)
    /// </param>
    public RtdsMetaContext(DbContextOptions<RtdsMetaContext> options, bool designTime = false) : base(options)
    {
        _designTime = designTime;
    }

    /// <summary>
    /// Configures the context options
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the options builder is not properly configured
    /// </exception>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .ReplaceService<IMigrationsAssembly,
                BaseContextMigrationAssembly<RtdsMetaContextSqlServer, RtdsMetaContextPostgres>>();
        if (!optionsBuilder.IsConfigured)
            throw new InvalidOperationException("OnConfiguring error");
    }

    /// <summary>
    /// Configures the model that was discovered by convention from the entity types
    /// exposed in <see cref="DbSet{TEntity}"/> properties on this context
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TagEntityConfiguration(Database, _designTime));
        modelBuilder.ApplyConfiguration(new InterfaceEntityConfiguration(Database, _designTime));
        modelBuilder.ApplyXmlDocumentation(_designTime);
    }
}

/// <summary>
/// Represents the SQL Server database context for user-related entities.
/// </summary>
public class RtdsMetaContextSqlServer(DbContextOptions<RtdsMetaContext> options, bool designTime = true)
    : RtdsMetaContext(options, designTime);

/// <summary>
/// Represents the PostgreSQL database context for user-related entities.
/// </summary>
public class RtdsMetaContextPostgres(DbContextOptions<RtdsMetaContext> options, bool designTime = true)
    : RtdsMetaContext(options, designTime);