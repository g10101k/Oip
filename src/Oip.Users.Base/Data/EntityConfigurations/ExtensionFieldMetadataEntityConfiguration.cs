using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Entities;
using Oip.Users.Base.Contexts;

namespace Oip.Users.Base.Data.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class ExtensionFieldMetadataEntityConfiguration : IEntityTypeConfiguration<ExtensionFieldMetadataEntity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ExtensionFieldMetadataEntity> entity)
    {
        entity.ToTable("ExtensionFieldMetadata", UserContext.SchemaName);
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedOnAdd();
        entity.Property(x => x.EntityCode).HasMaxLength(128).IsRequired();
        entity.Property(x => x.TableSchema).HasMaxLength(128).IsRequired();
        entity.Property(x => x.TableName).HasMaxLength(128).IsRequired();
        entity.Property(x => x.FieldName).HasMaxLength(128).IsRequired();
        entity.Property(x => x.DbColumn).HasMaxLength(128).IsRequired();
        entity.Property(x => x.OptionsJson);
        entity.HasIndex(x => new { x.EntityCode, x.FieldName }).IsUnique();
        entity.HasIndex(x => new { x.TableSchema, x.TableName, x.DbColumn }).IsUnique();
    }
}
