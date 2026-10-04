using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Extensions;
using Oip.Hitl.Data.Contexts;
using Oip.Hitl.Data.Entities;

namespace Oip.Hitl.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for <see cref="RegisteredToolEntity"/>
/// </summary>
/// <param name="database">The database facade</param>
public class RegisteredToolEntityConfiguration(DatabaseFacade database) : IEntityTypeConfiguration<RegisteredToolEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RegisteredToolEntity> builder)
    {
        builder.SetTableWithSchema(database, LlmContext.SchemaName);
        builder.HasKey(x => x.Name);
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.ParametersSchema).IsRequired();
        builder.Property(x => x.ActivityName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.TaskQueue).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.TaskQueue);
    }
}
