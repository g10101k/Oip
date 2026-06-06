using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for NotificationChannelEntity
/// </summary>
/// <param name="database">The database facade</param>
public class NotificationChannelEntityConfiguration(DatabaseFacade database)
    : IEntityTypeConfiguration<NotificationChannelEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationChannelEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(database, NotificationsDbContext.SchemaName);
        builder.HasKey(e => e.NotificationChannelId);
        builder.Property(e => e.NotificationChannelId)
            .ValueGeneratedOnAdd();
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(512);
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        builder.Property(e => e.RequiresVerification)
            .IsRequired()
            .HasDefaultValue(false);
        builder.Property(e => e.MaxRetryCount);
        builder.HasIndex(e => e.Name)
            .IsUnique();
        builder.HasIndex(e => e.Code)
            .IsUnique();
        builder.HasMany(e => e.UserPreferences)
            .WithOne(p => p.NotificationChannel)
            .HasForeignKey(p => p.NotificationChannelId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Deliveries)
            .WithOne(d => d.NotificationChannel)
            .HasForeignKey(d => d.NotificationChannelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}