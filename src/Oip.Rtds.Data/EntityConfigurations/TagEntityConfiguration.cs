using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Entities;
using Oip.Base.Data.Extensions;
using Oip.Rts.Base.Contexts;
using Oip.Rts.Base.Entities;

namespace Oip.Rts.Base.EntityConfigurations;

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
        entity.SetPrimaryKey(_designTime, e => e.TagId);
        entity.Property(e => e.TagId).ValueGeneratedOnAdd();
        entity.Property(e => e.Name).HasMaxLength(512);
    }
}