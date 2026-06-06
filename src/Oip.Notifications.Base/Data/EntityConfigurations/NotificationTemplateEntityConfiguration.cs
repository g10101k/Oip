using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for NotificationTemplateEntity
/// </summary>
/// <param name="database">The database facade</param>
public class NotificationTemplateEntityConfiguration(DatabaseFacade database)
    : IEntityTypeConfiguration<NotificationTemplateEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationTemplateEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(database, NotificationsDbContext.SchemaName);
        builder.HasKey(e => e.NotificationTemplateId);
        builder.Property(e => e.NotificationTemplateId)
            .ValueGeneratedOnAdd();
        builder.Property(e => e.SubjectTemplate)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(e => e.MessageTemplate)
            .IsRequired();
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        builder.Property(e => e.Importance)
            .IsRequired()
            .HasConversion<int>();
        
        // Relationships
        builder.HasOne(e => e.NotificationType)
            .WithMany(t => t.Templates)
            .HasForeignKey(e => e.NotificationTypeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.NotificationTemplateChannels)
            .WithOne(tc => tc.NotificationTemplate)
            .HasForeignKey(tc => tc.NotificationTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.NotificationTemplateUsers)
            .WithOne()
            .HasForeignKey(tu => tu.NotificationTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
        // Indexes
        builder.HasIndex(e => new { e.NotificationTypeId, e.IsActive });
    }
}