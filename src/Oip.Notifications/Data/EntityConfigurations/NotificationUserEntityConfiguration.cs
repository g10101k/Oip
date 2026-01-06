using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for NotificationUserEntity
/// </summary>
/// <param name="database">The database facade</param>
public class NotificationUserEntityConfiguration(DatabaseFacade database)
    : IEntityTypeConfiguration<NotificationUserEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationUserEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(database, NotificationsDbContext.SchemaName);
        builder.HasKey(e => e.NotificationUserId);
        builder.Property(e => e.NotificationUserId)
            .ValueGeneratedOnAdd();
        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(e => e.Message)
            .IsRequired();
        // Indexes
        builder.HasIndex(e => e.NotificationId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.NotificationId, e.UserId })
            .IsUnique();
        // Relationships
        builder.HasOne(e => e.Notification)
            .WithMany(n => n.NotificationUsers)
            .HasForeignKey(e => e.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Deliveries)
            .WithOne(d => d.NotificationUser)
            .HasForeignKey(d => d.NotificationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}