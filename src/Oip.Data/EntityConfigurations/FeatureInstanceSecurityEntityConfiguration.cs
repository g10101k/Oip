using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Entities;
using Oip.Data.Extensions;

namespace Oip.Data.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class FeatureInstanceSecurityEntityConfiguration : IEntityTypeConfiguration<FeatureInstanceSecurityEntity>
{
    private readonly DatabaseFacade _database;
    private readonly bool _designTime;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="database"></param>
    /// <param name="designTime"></param>
    public FeatureInstanceSecurityEntityConfiguration(DatabaseFacade database, bool designTime = false)
    {
        _database = database;
        _designTime = designTime;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity"></param>
    public void Configure(EntityTypeBuilder<FeatureInstanceSecurityEntity> entity)
    {
        entity.SetTable(_database);
        entity.SetPrimaryKey(_designTime, e => e.FeatureInstanceSecurityId);
        entity.Property(e => e.FeatureInstanceSecurityId).ValueGeneratedOnAdd();
        
        entity.Property(e => e.Right).HasMaxLength(255);
        entity.Property(e => e.Role).HasMaxLength(255);
    }
}