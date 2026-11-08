using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Entities;
using Oip.Base.Data.Extensions;

namespace Oip.Base.Data.EntityConfigurations;

/// <summary>
/// Configures persistence for the site-wide customization record.
/// </summary>
internal sealed class CustomizationEntityConfiguration(DatabaseFacade database)
    : IEntityTypeConfiguration<CustomizationEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CustomizationEntity> entity)
    {
        entity.SetTable(database);
        entity.HasKey(x => x.CustomizationId);
        entity.Property(x => x.CustomizationId).ValueGeneratedNever();
    }
}
