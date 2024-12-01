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
    /// Migration history table name
    /// </summary>
    public const string SchemaName = "oip";

    /// <summary>
    /// Features
    /// </summary>
    public DbSet<FeatureEntity> Features => Set<FeatureEntity>();

    /// <summary>
    /// FeaturesInstances
    /// </summary>
    public DbSet<FeatureInstanceEntity> FeaturesInstances => Set<FeatureInstanceEntity>();

    /// <summary>
    /// FeatureInstanceSecurities
    /// </summary>
    public DbSet<FeatureInstanceSecurityEntity> FeatureInstanceSecurities => Set<FeatureInstanceSecurityEntity>();

    /// <summary>
    /// FeatureSecurities
    /// </summary>
    public DbSet<FeatureSecurityEntity> FeatureSecurities => Set<FeatureSecurityEntity>();

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
        modelBuilder.ApplyConfiguration(new FeatureInstanceEntityConfiguration(Database, _designTime));
        modelBuilder.ApplyConfiguration(new FeatureInstanceSecurityEntityConfiguration(Database, _designTime));
        modelBuilder.ApplyConfiguration(new FeatureSecurityEntityConfiguration(Database, _designTime));
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
            case XpoProvider.MSSqlServer:
                builder.UseSqlServer(connectionString);
                using (var context = new SqlServerMigrationContext(builder.Options, false))
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
}

internal class SqlServerMigrationContext(DbContextOptions<OipContext> options, bool designTime)
    : OipContext(options, designTime);

internal class PostgresMigrationContext(DbContextOptions<OipContext> options, bool designTime)
    : OipContext(options, designTime);