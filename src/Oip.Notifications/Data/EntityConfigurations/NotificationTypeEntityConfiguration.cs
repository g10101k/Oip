using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;
using Oip.Notifications.Data.Contexts;
using Oip.Notifications.Data.Entities;

namespace Oip.Notifications.Data.EntityConfigurations;

/// <summary>
/// Configures database mapping for NotificationTypeEntity
/// </summary>
/// <param name="database">The database facade</param>
/// <param name="designTime">Whether this configuration is being used at design time</param>
public class NotificationTypeEntityConfiguration(DatabaseFacade database, bool designTime = false)
    : IEntityTypeConfiguration<NotificationTypeEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationTypeEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(database, NotificationsDbContext.SchemaName);
        builder.HasKey(e => e.NotificationTypeId);
        builder.Property(e => e.NotificationTypeId)
            .ValueGeneratedOnAdd();
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(512);
        builder.Property(e => e.Description)
            .HasMaxLength(1024);
        builder.Property(e => e.Scope)
            .IsRequired()
            .HasMaxLength(512);
        builder.HasIndex(e => e.Name)
            .IsUnique();
        builder.HasIndex(e => e.Scope);
        // Relationships
        builder.HasMany(e => e.Templates)
            .WithOne(t => t.NotificationType)
            .HasForeignKey(t => t.NotificationTypeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.UserPreferences)
            .WithOne(p => p.NotificationType)
            .HasForeignKey(p => p.NotificationTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}