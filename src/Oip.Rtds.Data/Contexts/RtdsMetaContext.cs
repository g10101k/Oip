using Microsoft.EntityFrameworkCore;
using Oip.Base.Data.Extensions;
using Oip.Rts.Base.Entities;
using Oip.Rts.Base.EntityConfigurations;

namespace Oip.Rts.Base.Contexts;

/// <summary>
/// 
/// </summary>
public class RtdsMetaContext : DbContext
{
    private readonly bool _designTime;

    /// <summary>
    /// Schema
    /// </summary>
    public const string SchemaName = "rtds";

    public const string MigrationHistoryTableName = "__MigrationHistory";

    public DbSet<TagEntity> Tags => Set<TagEntity>();

    /// <summary>
    /// .ctor
    /// </summary>
    public RtdsMetaContext(DbContextOptions<RtdsMetaContext> options, bool designTime = false) : base(options)
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
        modelBuilder.ApplyConfiguration(new TagEntityConfiguration(Database, _designTime));
        modelBuilder.ApplyXmlDocumentation(_designTime);
    }
}