using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Extensions;
using Oip.Hitl.Data.Contexts;
using Oip.Hitl.Data.Entities;

namespace Oip.Hitl.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for <see cref="AgentSkillEntity"/>
/// </summary>
/// <param name="database">The database facade</param>
public class AgentSkillEntityConfiguration(DatabaseFacade database) : IEntityTypeConfiguration<AgentSkillEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AgentSkillEntity> builder)
    {
        builder.SetTableWithSchema(database, LlmContext.SchemaName);
        builder.HasKey(x => new { x.AgentId, x.SkillId });
        builder.HasOne(x => x.Agent).WithMany(x => x.Skills).HasForeignKey(x => x.AgentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Skill).WithMany(x => x.Agents).HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
