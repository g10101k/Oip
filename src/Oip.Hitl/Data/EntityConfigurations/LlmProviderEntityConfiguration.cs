using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Extensions;
using Oip.Hitl.Data.Contexts;
using Oip.Hitl.Data.Entities;

namespace Oip.Hitl.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for <see cref="LlmProviderEntity"/>
/// </summary>
/// <param name="database">The database facade</param>
public class LlmProviderEntityConfiguration(DatabaseFacade database) : IEntityTypeConfiguration<LlmProviderEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LlmProviderEntity> builder)
    {
        builder.SetTableWithSchema(database, LlmContext.SchemaName);
        builder.HasKey(x => x.LlmProviderId);
        builder.Property(x => x.LlmProviderId).ValueGeneratedOnAdd();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.BaseUrl).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Model).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ProviderType).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(x => x.Name).IsUnique();

        // At most one default provider.
        var defaultIndex = builder.HasIndex(x => x.IsDefault).IsUnique();
        if (database.IsNpgsql())
            defaultIndex.HasFilter("\"IsDefault\" = true");
        else if (database.IsSqlServer())
            defaultIndex.HasFilter("[IsDefault] = 1");
    }
}
