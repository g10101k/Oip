using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Entities;
using Oip.Data.Extensions;

namespace Oip.Data.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class FeatureEntityConfiguration : IEntityTypeConfiguration<FeatureEntity>
{
    private readonly DatabaseFacade _database;
    private readonly bool _designTime;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="database"></param>
    /// <param name="designTime"></param>
    public FeatureEntityConfiguration(DatabaseFacade database, bool designTime = false)
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
        entity.SetTable(_database);
        entity.SetPrimaryKey(_designTime, e => e.FeatureId);
        entity.Property(e => e.FeatureId).ValueGeneratedOnAdd();
        entity.Property(e => e.Name).HasMaxLength(512);
        
        if (_designTime)
        {
            entity.Ignore(e => e.FeatureSecurities);
        }
        else
        {
            entity.HasMany(e => e.FeatureSecurities).WithOne(e => e.Feature);
        }
    }
}