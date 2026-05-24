using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;

namespace Oip.Applications.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for application registry items.
/// </summary>
public class ApplicationRegistryItemEntityConfiguration(DatabaseFacade database)
    : IEntityTypeConfiguration<ApplicationRegistryItemEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApplicationRegistryItemEntity> builder)
    {
        builder.SetTableWithSchema(database, ApplicationRegistryDbContext.SchemaName);
        builder.HasKey(e => e.ApplicationRegistryItemId);
        builder.Property(e => e.ApplicationRegistryItemId)
            .ValueGeneratedOnAdd();
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(128);
        builder.Property(e => e.DisplayName)
            .IsRequired()
            .HasMaxLength(512);
        builder.Property(e => e.BaseUrl)
            .IsRequired()
            .HasMaxLength(2048);
        builder.Property(e => e.ApiBaseUrl)
            .IsRequired()
            .HasMaxLength(2048);
        builder.Property(e => e.Icon)
            .IsRequired()
            .HasMaxLength(256);
        builder.Property(e => e.Order)
            .IsRequired();
        builder.Property(e => e.Enabled)
            .IsRequired()
            .HasDefaultValue(true);
        builder.HasIndex(e => e.Code)
            .IsUnique();
    }
}
