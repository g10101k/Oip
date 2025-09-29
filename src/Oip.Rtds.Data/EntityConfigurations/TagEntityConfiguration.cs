using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Extensions;
using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Entities;

namespace Oip.Rtds.Data.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class TagEntityConfiguration : IEntityTypeConfiguration<TagEntity>
{
    private readonly DatabaseFacade _database;
    private readonly bool _designTime;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    /// <param name="designTime"></param>
    public TagEntityConfiguration(DatabaseFacade database, bool designTime = false)
    {
        _database = database;
        _designTime = designTime;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity"></param>
    public void Configure(EntityTypeBuilder<TagEntity> entity)
    {
        entity.SetTableWithSchema(_database, RtdsMetaContext.SchemaName);
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Name).IsUnique();
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.Name).HasMaxLength(512);
        entity.Property(e => e.InterfaceId).HasMaxLength(128);
        entity.Property(e => e.Creator).HasMaxLength(256);
        entity.Property(e => e.Descriptor).HasMaxLength(1024);
    }
}
