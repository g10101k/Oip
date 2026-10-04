using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Extensions;
using Oip.Hitl.Data.Contexts;
using Oip.Hitl.Data.Entities;

namespace Oip.Hitl.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for <see cref="SkillEntity"/>
/// </summary>
/// <param name="database">The database facade</param>
public class SkillEntityConfiguration(DatabaseFacade database) : IEntityTypeConfiguration<SkillEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SkillEntity> builder)
    {
        builder.SetTableWithSchema(database, LlmContext.SchemaName);
        builder.HasKey(x => x.SkillId);
        builder.Property(x => x.SkillId).ValueGeneratedOnAdd();
        builder.Property(x => x.Code).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Instructions).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}
