using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Extensions;
using Oip.Hitl.Data.Contexts;
using Oip.Hitl.Data.Entities;

namespace Oip.Hitl.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for <see cref="AgentEntity"/>
/// </summary>
/// <param name="database">The database facade</param>
public class AgentEntityConfiguration(DatabaseFacade database) : IEntityTypeConfiguration<AgentEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AgentEntity> builder)
    {
        builder.SetTableWithSchema(database, LlmContext.SchemaName);
        builder.HasKey(x => x.AgentId);
        builder.Property(x => x.AgentId).ValueGeneratedOnAdd();
        builder.Property(x => x.Code).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Code).IsUnique();
        // An agent of a deleted provider falls back to the default provider.
        builder.HasOne(x => x.LlmProvider).WithMany().HasForeignKey(x => x.LlmProviderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
