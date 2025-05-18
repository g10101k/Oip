using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Extensions;
using Oip.Example.Data.Contexts;
using Oip.Example.Data.Entities;

namespace Oip.Example.Data.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class ExampleEntityConfiguration : IEntityTypeConfiguration<ExampleEntity>
{
    private readonly DatabaseFacade _database;
    private readonly bool _designTime;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    /// <param name="designTime"></param>
    public ExampleEntityConfiguration(DatabaseFacade database, bool designTime = false)
    {
        _database = database;
        _designTime = designTime;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity"></param>
    public void Configure(EntityTypeBuilder<ExampleEntity> entity)
    {
        entity.SetTable(_database);
        entity.SetPrimaryKey(_designTime, e => e.ExampleId);
        entity.HasKey(x => x.ExampleId);
        entity.Property(x => x.ExampleId).ValueGeneratedOnAdd();
        entity.Property(x => x.Name).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(50);
        entity.HasData(new List<ExampleEntity>
        {
            new ExampleEntity
            {
                ExampleId = 1,
                Name = "name 1",
            }
        });
    }
}