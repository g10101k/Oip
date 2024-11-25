using Microsoft.EntityFrameworkCore;
using Oip.Data.Entities;
using Oip.Data.EntityConfigurations;

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
    public OipContext(DbContextOptions<OipContext> options) : base(options)
    {
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
        modelBuilder.ApplyConfiguration(new FeatureEntityConfiguration(Database));
    }
}