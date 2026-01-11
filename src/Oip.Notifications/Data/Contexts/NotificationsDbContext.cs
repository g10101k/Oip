using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Oip.Data.Contexts;
using Oip.Data.EntityConfigurations;
using Oip.Data.Extensions;
using Oip.Notifications.Data.Entities;
using Oip.Notifications.Data.EntityConfigurations;

namespace Oip.Notifications.Data.Contexts;

/// <summary>
/// Database context for notification entities
/// </summary>
/// <param name="options">The options for this context</param>
/// <param name="designTime">Whether this context is being used at design time</param>
public class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options, bool designTime = false)
    : DbContext(options), IDataProtectionKeyContext
{
    /// <summary>
    /// Schema name for notification entities
    /// </summary>
    public const string SchemaName = "notifications";

    /// <summary>
    /// Table name for tracking Entity Framework Core migrations
    /// </summary>
    public const string MigrationHistoryTableName = "__EFMigrationHistory";

    /// <summary>
    /// Data protection keys
    /// </summary>
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    /// <summary>
    /// Collection of notification types
    /// </summary>
    public DbSet<NotificationTypeEntity> NotificationTypes { get; set; }

    /// <summary>
    /// Available notification delivery channels
    /// </summary>
    public DbSet<NotificationChannelEntity> NotificationChannels { get; set; }

    /// <summary>
    /// Collection of notification templates
    /// </summary>
    public DbSet<NotificationTemplateEntity> NotificationTemplates { get; set; }

    /// <summary>
    /// Associations between notification templates and notification channels
    /// </summary>
    public DbSet<NotificationTemplateChannelEntity> NotificationTemplateChannels { get; set; }

    /// <summary>
    /// Entity set for notification template users
    /// </summary>
    public DbSet<NotificationTemplateUserEntity> NotificationTemplateUsers { get; set; }

    /// <summary>
    /// User preferences for receiving notifications
    /// </summary>
    public DbSet<UserNotificationPreferenceEntity> UserNotificationPreferences { get; set; }

    /// <summary>
    /// Collection of notifications
    /// </summary>
    public DbSet<NotificationEntity> Notifications { get; set; }

    /// <summary>
    /// Users associated with notifications
    /// </summary>
    public DbSet<NotificationUserEntity> NotificationUsers { get; set; }

    /// <summary>
    /// Delivery history of notifications to users through various channels
    /// </summary>
    public DbSet<NotificationDeliveryEntity> NotificationDeliveries { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new NotificationTypeEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationChannelEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationTemplateEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationTemplateChannelEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationTemplateUserEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new UserNotificationPreferenceEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationUserEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationDeliveryEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new DataProtectionKeyEntityConfiguration(Database, SchemaName));

        modelBuilder.ApplyXmlDocumentation(designTime);
    }

    /// <summary>
    /// Configures the context options by replacing the migration assembly service
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the options builder is not properly configured
    /// </exception>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .ReplaceService<IMigrationsAssembly,
                BaseContextMigrationAssembly<NotificationsDbContextSqlServer, NotificationsDbContextPostgres>>();
        if (!optionsBuilder.IsConfigured)
            throw new InvalidOperationException("OnConfiguring error");
    }
}


/// <summary>
/// Represents the SQL Server database context for notification-related entities
/// </summary>
/// <param name="options">The options for this context</param>
/// <param name="designTime">Whether this context is being used at design time</param>
public class NotificationsDbContextSqlServer(DbContextOptions<NotificationsDbContext> options, bool designTime = true)
    : NotificationsDbContext(options, designTime);

/// <summary>
/// Represents the PostgreSQL database context for notification-related entities
/// </summary>
/// <param name="options">The options for this context</param>
/// <param name="designTime">Whether this context is being used at design time</param>
public class NotificationsDbContextPostgres(DbContextOptions<NotificationsDbContext> options, bool designTime = true)
    : NotificationsDbContext(options, designTime);