using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for NotificationTemplateUserEntity
/// </summary>
/// <param name="database">The database facade</param>
public class NotificationTemplateUserEntityConfiguration(DatabaseFacade database)
    : IEntityTypeConfiguration<NotificationTemplateUserEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationTemplateUserEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(database, NotificationsDbContext.SchemaName);
        builder.HasKey(e => e.NotificationTemplateUserId);
        builder.Property(e => e.NotificationTemplateUserId)
            .ValueGeneratedOnAdd();
        // Composite unique index
        builder.HasIndex(e => new { e.NotificationTemplateId, e.UserId })
            .IsUnique();
        builder.HasIndex(e => e.UserId);
    }
}