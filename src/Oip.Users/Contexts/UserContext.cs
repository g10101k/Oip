using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Oip.Base.Data.Extensions;
using Oip.Users.Entities;
using Oip.Users.EntityConfigurations;

namespace Oip.Users.Contexts;

/// <summary>
/// Represents the database context for user-related entities.
/// </summary>
public class UserContext : DbContext
{
    private readonly bool _designTime;

    /// <summary>
    /// The name of the database schema used for user entities
    /// </summary>
    public const string SchemaName = "usr";

    /// <summary>
    /// The name of the table used to store EF Core migration history
    /// </summary>
    public const string MigrationHistoryTableName = "__MigrationHistory";

    /// <summary>
    /// Gets the DbSet for managing user entities
    /// </summary>
    public DbSet<UserEntity> Users { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContext"/> class with the specified options and design time flag
    /// </summary>
    /// <param name="options">The options to be used by this context</param>
    /// <param name="designTime">
    /// Flag indicating whether the context is being created at design time (e.g. for migrations)
    /// </param>
    public UserContext(DbContextOptions<UserContext> options, bool designTime = false) : base(options)
    {
        _designTime = designTime;
    }

    /// <summary>
    /// Configures the context options by replacing the migration assembly service
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the options builder is not properly configured
    /// </exception>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ReplaceService<IMigrationsAssembly, UserContextMigrationAssembly>();

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
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration(Database, _designTime));

        modelBuilder.ApplyXmlDocumentation(_designTime);
    }

    /// <summary>
    /// Saves all changes made in this context to the database with automatic timestamp updates
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete</param>
    /// <returns>A task that represents the asynchronous save operation</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is UserEntity &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                ((UserEntity)entityEntry.Entity).CreatedAt = DateTimeOffset.UtcNow;
            }

            ((UserEntity)entityEntry.Entity).UpdatedAt = DateTimeOffset.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Represents the SQL Server database context for user-related entities.
/// </summary>
public class UserContextSqlServer(DbContextOptions<UserContext> options, bool designTime = false)
    : UserContext(options, designTime);

/// <summary>
/// Represents the PostgreSQL database context for user-related entities.
/// </summary>
public class UserContextPostgres(DbContextOptions<UserContext> options, bool designTime = false)
    : UserContext(options, designTime);