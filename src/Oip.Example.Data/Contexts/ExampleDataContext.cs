using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Oip.Example.Data.Entities;
using Oip.Example.Data.EntityConfigurations;

namespace Oip.Example.Data.Contexts;

/// <summary>
/// Example data context
/// </summary>
public class ExampleDataContext : DbContext
{
    public const string MigrationHistoryTableName = "__MigrationHistoryExampleDataContext";
    public const string MigrationHistorySchemaName = "oip";
    private readonly bool _designTime;

    /// <summary>
    /// Example entity
    /// </summary>
    public DbSet<ExampleEntity> Examples => Set<ExampleEntity>();

    /// <summary>
    /// .ctor
    /// </summary>
    public ExampleDataContext(DbContextOptions<ExampleDataContext> options, bool designTime = false) : base(options)
    {
        _designTime = designTime;
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ExampleEntityConfiguration(Database, _designTime));
    }
}