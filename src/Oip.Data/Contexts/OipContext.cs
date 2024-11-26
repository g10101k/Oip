using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    /// Migration history table name
    /// </summary>
    public const string SchemaName = "oip";

    /// <summary>
    /// Features
    /// </summary>
    public DbSet<FeatureEntity> Features => Set<FeatureEntity>();

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
        modelBuilder.ApplyConfiguration(new FeatureEntityConfiguration(Database, _designTime));
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
                }

                break;
            case XpoProvider.SQLite:
                builder.UseSqlite(connectionString);
                using (var context = new SqliteMigrationContext(builder.Options, false))
                {
                    context.Database.Migrate();
                }

                break;
            case XpoProvider.MSSqlServer:
                builder.UseSqlServer(connectionString);
                using (var context = new MsSqlServerMigrationContext(builder.Options, false))
                {
                    context.Database.Migrate();
                }

                break;
            case XpoProvider.InMemoryDataStore:
                break;
            default:
                throw new InvalidOperationException("Unknown provider");
        }
    }


    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ProcessSqliteIdentities(ChangeTracker.Entries());
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ProcessSqliteIdentities(ChangeTracker.Entries());
        return base.SaveChangesAsync(cancellationToken);
    }


    private void ProcessSqliteIdentities(IEnumerable<EntityEntry> entries)
    {
        if (!Database.IsSqlite())
            return;

        _ = entries
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity switch
            {
                FeatureEntity c => c.FeatureId = Set<FeatureEntity>().Any()
                    ? Set<FeatureEntity>().Max(x => x.FeatureId) + 1
                    : 1,
                _ => (object)null!
            })
            .ToList();
    }
}

internal class MsSqlServerMigrationContext(DbContextOptions<OipContext> options, bool designTime)
    : OipContext(options, designTime);

internal class PostgresMigrationContext(DbContextOptions<OipContext> options, bool designTime)
    : OipContext(options, designTime);

internal class SqliteMigrationContext(DbContextOptions<OipContext> options, bool designTime)
    : OipContext(options, designTime);