using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Entities;
using Oip.Data.Extensions;

namespace Oip.Data.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class FeatureInstanceEntityConfiguration : IEntityTypeConfiguration<FeatureInstanceEntity>
{
    private readonly DatabaseFacade _database;
    private readonly bool _designTime;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="database"></param>
    /// <param name="designTime"></param>
    public FeatureInstanceEntityConfiguration(DatabaseFacade database, bool designTime = false)
    {
        _database = database;
        _designTime = designTime;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity"></param>
    public void Configure(EntityTypeBuilder<FeatureInstanceEntity> entity)
    {
        entity.SetTable(_database);
        entity.SetPrimaryKey(_designTime, e => e.FeatureInstanceId);
        entity.Property(e => e.FeatureInstanceId).ValueGeneratedOnAdd();

        entity.Property(e => e.Icon).HasMaxLength(64);
        entity.Property(e => e.Label).HasMaxLength(128);
        entity.Property(e => e.Url).HasMaxLength(1024);
        entity.Property(e => e.Target).HasMaxLength(64);

        if (_designTime)
        {
            entity.Ignore(e => e.Parent);
            entity.Ignore(e => e.Feature);
            entity.Ignore(e => e.Securities);
        }
        else
        {
            entity.HasOne(e => e.Parent).WithMany(x => x.Items).HasForeignKey(e => e.ParentId);
            entity.HasMany(e => e.Securities).WithOne(x=>x.FeatureInstance).HasForeignKey(x=>x.FeatureInstanceId);
        }
    }
}