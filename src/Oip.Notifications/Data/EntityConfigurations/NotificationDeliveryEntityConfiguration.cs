using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for NotificationDeliveryEntity
/// </summary>
/// <param name="database">The database facade</param>
public class NotificationDeliveryEntityConfiguration(DatabaseFacade database)
    : IEntityTypeConfiguration<NotificationDeliveryEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationDeliveryEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(database, NotificationsDbContext.SchemaName);
        builder.HasKey(e => e.NotificationDeliveryId);
        builder.Property(e => e.NotificationDeliveryId)
            .ValueGeneratedOnAdd();
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Enums.DeliveryStatus.Pending);
        builder.Property(e => e.ExternalId)
            .HasMaxLength(200);
        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);
        builder.Property(e => e.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        builder.Property(e => e.SentAt);
        builder.Property(e => e.DeliveredAt);
        // Indexes
        builder.HasIndex(e => e.NotificationUserId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.NotificationChannelId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.SentAt);
        builder.HasIndex(e => new { e.Status, e.RetryCount });
        builder.HasIndex(e => e.ExternalId);
        // Relationships
        builder.HasOne(e => e.NotificationUser)
            .WithMany(u => u.Deliveries)
            .HasForeignKey(e => e.NotificationUserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.NotificationChannel)
            .WithMany(c => c.Deliveries)
            .HasForeignKey(e => e.NotificationChannelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}