using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for NotificationEntity
/// </summary>
/// <param name="database">The database facade</param>
public class NotificationEntityConfiguration(DatabaseFacade database) : IEntityTypeConfiguration<NotificationEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(database, NotificationsDbContext.SchemaName);
        builder.HasKey(e => e.NotificationId);
        builder.Property(e => e.NotificationId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.CreatedAt)
            .IsRequired();
        builder.Property(e => e.DataJson);
        // Indexes
        builder.HasIndex(e => e.NotificationTypeId);
        builder.HasIndex(e => e.CreatedAt);
        // Relationships
        builder.HasOne(e => e.NotificationType)
            .WithMany()
            .HasForeignKey(e => e.NotificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.NotificationUsers)
            .WithOne(u => u.Notification)
            .HasForeignKey(u => u.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}