using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Entities;
using Oip.Base.Data.Extensions;

namespace Oip.Base.Data.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class ModuleSecurityEntityConfiguration : IEntityTypeConfiguration<ModuleSecurityEntity>
{
    private readonly DatabaseFacade _database;
    private readonly bool _designTime;


    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    /// <param name="designTime"></param>
    public ModuleSecurityEntityConfiguration(DatabaseFacade database, bool designTime = false)
    {
        _database = database;
        _designTime = designTime;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity"></param>
    public void Configure(EntityTypeBuilder<ModuleSecurityEntity> entity)
    {
        entity.SetTable(_database);
        entity.SetPrimaryKey(_designTime, e => e.ModuleSecurityId);
        entity.Property(e => e.ModuleSecurityId).ValueGeneratedOnAdd();

        entity.Property(e => e.Right).HasMaxLength(255);
        entity.Property(e => e.Role).HasMaxLength(255);

        if (_designTime)
        {
            entity.Ignore(e => e.Module);
        }
    }
}