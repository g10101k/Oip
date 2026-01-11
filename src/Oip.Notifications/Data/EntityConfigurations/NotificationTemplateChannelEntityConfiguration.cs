using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for NotificationTemplateChannelEntity
/// </summary>
/// <param name="database">The database facade</param>
public class NotificationTemplateChannelEntityConfiguration(DatabaseFacade database)
    : IEntityTypeConfiguration<NotificationTemplateChannelEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationTemplateChannelEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(database, NotificationsDbContext.SchemaName);
        builder.HasKey(e => e.NotificationTemplateChannelId);
        builder.Property(e => e.NotificationTemplateChannelId)
            .ValueGeneratedOnAdd();
        // Composite unique index to prevent duplicate relationships
        builder.HasIndex(e => new { e.NotificationTemplateId, e.NotificationChannelId })
            .IsUnique();
    }
}