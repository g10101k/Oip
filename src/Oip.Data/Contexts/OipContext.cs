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
        modelBuilder.ApplyConfiguration(new FeatureEntityConfiguration(Database, _designTime));
        modelBuilder.ApplyConfiguration(new FeatureSecurityEntityConfiguration(Database, _designTime));
        base.OnModelCreating(modelBuilder);
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
                    AddData(context);
                }

                break;
            case XpoProvider.MSSqlServer:
                builder.UseSqlServer(connectionString);
                using (var context = new MsSqlServerMigrationContext(builder.Options, false))
                {
                    AddData(context);
                }

                break;
            case XpoProvider.InMemoryDataStore:
                break;
            default:
                throw new InvalidOperationException("Unknown provider");
        }
    }

    private static void AddData(OipContext context)
    {
        context.Database.Migrate();

        if (!context.Features.Any())
        {
            context.Features.AddRange(
                new FeatureEntity
                {
                    Name = "Test1",
                    Settings = "{}",
                    FeatureSecurities = new List<FeatureSecurityEntity>(){
                        new FeatureSecurityEntity(){ Right="Read", Role="ReadOnly"},
                        new FeatureSecurityEntity(){ Right="Write", Role="Write"},
                    }
                },
                new FeatureEntity
                {
                    Name = "Test2",
                    Settings = "{}",
                    FeatureSecurities = new List<FeatureSecurityEntity>(){
                        new FeatureSecurityEntity(){ Right="Read", Role="ReadOnly"},
                        new FeatureSecurityEntity(){ Right="Write", Role="Write"},
                    }
                },
                new FeatureEntity
                {
                    Name = "Test2",
                    Settings = "{}",
                    FeatureSecurities = new List<FeatureSecurityEntity>(){
                        new FeatureSecurityEntity(){ Right="Read", Role="ReadOnly"},
                        new FeatureSecurityEntity(){ Right="Write", Role="Write"},
                    }
                }
            );
            context.SaveChanges();
        }
    }
}

internal class MsSqlServerMigrationContext(DbContextOptions<OipContext> options, bool designTime)
    : OipContext(options, designTime);

internal class PostgresMigrationContext(DbContextOptions<OipContext> options, bool designTime)
    : OipContext(options, designTime);
