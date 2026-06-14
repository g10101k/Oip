using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Entities;
using Oip.Base.Data.Extensions;

namespace Oip.Base.Data.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class ModuleInstanceEntityConfiguration : IEntityTypeConfiguration<ModuleInstanceEntity>
{
    private readonly DatabaseFacade _database;
    private readonly bool _designTime;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    /// <param name="designTime"></param>
    public ModuleInstanceEntityConfiguration(DatabaseFacade database, bool designTime = false)
    {
        _database = database;
        _designTime = designTime;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity"></param>
    public void Configure(EntityTypeBuilder<ModuleInstanceEntity> entity)
    {
        entity.SetTable(_database);
        entity.SetPrimaryKey(_designTime, e => e.ModuleInstanceId);
        entity.Property(e => e.ModuleInstanceId).ValueGeneratedOnAdd();

        entity.Property(e => e.Icon).HasMaxLength(64);
        entity.Property(e => e.Label).HasMaxLength(128);
        entity.Property(e => e.Url).HasMaxLength(1024);
        entity.Property(e => e.Target).HasMaxLength(64);

        if (_designTime)
        {
            entity.Ignore(e => e.Parent);
            entity.Ignore(e => e.Module);
            entity.Ignore(e => e.Securities);
        }
        else
        {
            entity.HasOne(e => e.Parent).WithMany(x => x.Items).HasForeignKey(e => e.ParentId);
            entity.HasMany(e => e.Securities).WithOne(x=>x.ModuleInstance).HasForeignKey(x=>x.ModuleInstanceId);
        }
    }
}