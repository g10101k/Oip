using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for UserNotificationPreferenceEntity
/// </summary>
/// <param name="database">The database facade</param>
public class UserNotificationPreferenceEntityConfiguration(DatabaseFacade database)
    : IEntityTypeConfiguration<UserNotificationPreferenceEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserNotificationPreferenceEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(database, NotificationsDbContext.SchemaName);
        builder.HasKey(e => e.UserNotificationPreferenceId);
        builder.Property(e => e.UserNotificationPreferenceId)
            .ValueGeneratedOnAdd();
        builder.Property(e => e.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);
        // Composite unique index to prevent duplicate settings
        builder.HasIndex(e => new { e.UserId, e.NotificationTypeId, e.NotificationChannelId })
            .IsUnique();
        // Indexes for fast search
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.NotificationTypeId);
        builder.HasIndex(e => new { e.UserId, e.IsEnabled });
        // Relationships
        builder.HasOne(e => e.NotificationType)
            .WithMany(t => t.UserPreferences)
            .HasForeignKey(e => e.NotificationTypeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.NotificationChannel)
            .WithMany(c => c.UserPreferences)
            .HasForeignKey(e => e.NotificationChannelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}