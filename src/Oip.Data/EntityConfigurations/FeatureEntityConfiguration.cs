using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Entities;

namespace Oip.Data.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class FeatureEntityConfiguration : IEntityTypeConfiguration<FeatureEntity>
{
    private const string TableName = "Feature";
    private const string SchemaName = "oip";
    private readonly DatabaseFacade _database;
    private readonly bool _designTime;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="database"></param>
    /// <param name="designTime"></param>
    public FeatureEntityConfiguration(DatabaseFacade database, bool designTime)
    {
        _database = database;
        _designTime = designTime;
    }
    
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity"></param>
    public void Configure(EntityTypeBuilder<FeatureEntity> entity)
    {
        if (_designTime)
        {
            entity.HasNoKey();
        }
        else
        {
            entity.HasKey(x => x.FeatureId);
        }

        entity.Property(e => e.FeatureId).ValueGeneratedOnAdd();

        if (_database.IsNpgsql())
            entity.ToTable(TableName, SchemaName);
        else if (_database.IsSqlServer())
            entity.ToTable(TableName, SchemaName);
        else
            entity.ToTable(TableName);
    }
}