using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Entities;
using Oip.Base.Data.Extensions;

namespace Oip.Base.Data.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class ModuleEntityConfiguration : IEntityTypeConfiguration<ModuleEntity>
{
    private readonly DatabaseFacade _database;
    private readonly bool _designTime;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    /// <param name="designTime"></param>
    public ModuleEntityConfiguration(DatabaseFacade database, bool designTime = false)
    {
        _database = database;
        _designTime = designTime;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity"></param>
    public void Configure(EntityTypeBuilder<ModuleEntity> entity)
    {
        entity.SetTable(_database);
        entity.SetPrimaryKey(_designTime, e => e.ModuleId);
        entity.Property(e => e.ModuleId).ValueGeneratedOnAdd();
        entity.Property(e => e.Name).HasMaxLength(512);
        entity.Property(e => e.RouterLink).HasMaxLength(256);

        if (_designTime)
        {
            entity.Ignore(e => e.ModuleSecurities);
            entity.Ignore(e => e.ModuleInstances);
        }
        else
        {
            entity.HasMany(e => e.ModuleSecurities).WithOne(e => e.Module);
            entity.HasMany(e => e.ModuleInstances).WithOne(e => e.Module);
        }
    }
}