using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Extensions;
using Oip.Hitl.Data.Contexts;
using Oip.Hitl.Data.Entities;

namespace Oip.Hitl.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for <see cref="SkillToolEntity"/>
/// </summary>
/// <param name="database">The database facade</param>
public class SkillToolEntityConfiguration(DatabaseFacade database) : IEntityTypeConfiguration<SkillToolEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SkillToolEntity> builder)
    {
        builder.SetTableWithSchema(database, LlmContext.SchemaName);
        builder.HasKey(x => new { x.SkillId, x.ToolName });
        builder.Property(x => x.ToolName).HasMaxLength(100);
        builder.HasOne(x => x.Skill).WithMany(x => x.Tools).HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
