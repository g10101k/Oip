using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;

namespace Oip.Notifications.Entities;

/// <summary>
/// Represents the database context for notification entities
/// </summary>
public class NotificationsDbContext : DbContext
{
    /// <summary>
    /// Defines the schema name for the notification entities
    /// </summary>
    public const string SchemaName = "notifications";

    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<NotificationTypeEntity> NotificationTypes { get; set; }
    public DbSet<NotificationChannelEntity> NotificationChannels { get; set; }
    public DbSet<NotificationTemplateEntity> NotificationTemplates { get; set; }
    public DbSet<NotificationTemplateChannelEntity> NotificationTemplateChannels { get; set; }
    public DbSet<NotificationTemplateUserEntity> NotificationTemplateUsers { get; set; }
    public DbSet<UserNotificationPreferenceEntity> UserNotificationPreferences { get; set; }
    public DbSet<NotificationEntity> Notifications { get; set; }
    public DbSet<NotificationUserEntity> NotificationUsers { get; set; }
    public DbSet<NotificationDeliveryEntity> NotificationDeliveries { get; set; }

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
    }
}

/// <inheritdoc />
public class NotificationTypeEntityConfiguration : IEntityTypeConfiguration<NotificationTypeEntity>
{
    private readonly DatabaseFacade _database;
    private readonly bool _designTime;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    /// <param name="designTime"></param>
    public NotificationTypeEntityConfiguration(DatabaseFacade database, bool designTime = false)
    {
        _database = database;
        _designTime = designTime;
    }

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationTypeEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(_database, NotificationsDbContext.SchemaName);

        builder.HasKey(e => e.NotificationTypeId);

        builder.Property(e => e.NotificationTypeId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Scope)
            .IsRequired()
            .HasMaxLength(50);

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

public class NotificationChannelEntityConfiguration : IEntityTypeConfiguration<NotificationChannelEntity>
{
    private readonly DatabaseFacade _database;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    public NotificationChannelEntityConfiguration(DatabaseFacade database)
    {
        _database = database;
    }

    public void Configure(EntityTypeBuilder<NotificationChannelEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(_database, NotificationsDbContext.SchemaName);

        builder.HasKey(e => e.NotificationChannelId);

        builder.Property(e => e.NotificationChannelId)
            .ValueGeneratedOnAdd();

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

public class NotificationTemplateEntityConfiguration : IEntityTypeConfiguration<NotificationTemplateEntity>
{
    private readonly DatabaseFacade _database;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    public NotificationTemplateEntityConfiguration(DatabaseFacade database)
    {
        _database = database;
    }

    public void Configure(EntityTypeBuilder<NotificationTemplateEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(_database, NotificationsDbContext.SchemaName);

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

public class NotificationTemplateChannelEntityConfiguration :
    IEntityTypeConfiguration<NotificationTemplateChannelEntity>
{
    private readonly DatabaseFacade _database;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    public NotificationTemplateChannelEntityConfiguration(DatabaseFacade database)
    {
        _database = database;
    }

    public void Configure(EntityTypeBuilder<NotificationTemplateChannelEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(_database, NotificationsDbContext.SchemaName);

        builder.HasKey(e => e.NotificationTemplateChannelId);

        builder.Property(e => e.NotificationTemplateChannelId)
            .ValueGeneratedOnAdd();

        // Composite unique index to prevent duplicate relationships
        builder.HasIndex(e => new { e.NotificationTemplateId, e.NotificationChannelId })
            .IsUnique();

        // Relationships
        builder.HasOne(e => e.NotificationTemplate)
            .WithMany(t => t.NotificationTemplateChannels)
            .HasForeignKey(e => e.NotificationTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.NotificationChannel)
            .WithMany(c => c.Templates)
            .HasForeignKey(e => e.NotificationChannelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class NotificationTemplateUserEntityConfiguration : IEntityTypeConfiguration<NotificationTemplateUserEntity>
{
    private readonly DatabaseFacade _database;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    public NotificationTemplateUserEntityConfiguration(DatabaseFacade database)
    {
        _database = database;
    }

    public void Configure(EntityTypeBuilder<NotificationTemplateUserEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(_database, NotificationsDbContext.SchemaName);

        builder.HasKey(e => e.NotificationTemplateUserId);

        builder.Property(e => e.NotificationTemplateUserId)
            .ValueGeneratedOnAdd();

        // Composite unique index
        builder.HasIndex(e => new { e.NotificationTemplateId, e.UserId })
            .IsUnique();

        builder.HasIndex(e => e.UserId);
    }
}

public class UserNotificationPreferenceEntityConfiguration : IEntityTypeConfiguration<UserNotificationPreferenceEntity>
{
    private readonly DatabaseFacade _database;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    public UserNotificationPreferenceEntityConfiguration(DatabaseFacade database)
    {
        _database = database;
    }

    public void Configure(EntityTypeBuilder<UserNotificationPreferenceEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(_database, NotificationsDbContext.SchemaName);

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

public class NotificationEntityConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    private readonly DatabaseFacade _database;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    public NotificationEntityConfiguration(DatabaseFacade database)
    {
        _database = database;
    }

    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(_database, NotificationsDbContext.SchemaName);

        builder.HasKey(e => e.NotificationId);

        builder.Property(e => e.NotificationId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Importance)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.DataJson);

        // Indexes
        builder.HasIndex(e => e.NotificationTypeId);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.Importance);

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

public class NotificationUserEntityConfiguration : IEntityTypeConfiguration<NotificationUserEntity>
{
    private readonly DatabaseFacade _database;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    public NotificationUserEntityConfiguration(DatabaseFacade database)
    {
        _database = database;
    }

    public void Configure(EntityTypeBuilder<NotificationUserEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(_database, NotificationsDbContext.SchemaName);

        builder.HasKey(e => e.NotificationUserId);

        builder.Property(e => e.NotificationUserId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

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

public class NotificationDeliveryEntityConfiguration(DatabaseFacade database)
    : IEntityTypeConfiguration<NotificationDeliveryEntity>
{
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
            .HasDefaultValue(DeliveryStatus.Pending);

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

/// <summary>
/// Represents a type of notification within the system
/// </summary>
public class NotificationTypeEntity
{
    /// <summary>
    /// Defines the unique identifier for the notification type
    /// </summary>
    public int NotificationTypeId { get; set; }

    /// <summary>
    /// Name of the notification type
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Provides a detailed explanation of the notification type
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Scope of notification type (global, appName, feature)
    /// </summary>
    public string Scope { get; set; }

    /// <summary>
    /// Represents the collection of notification templates associated with a specific notification type
    /// </summary>
    public ICollection<NotificationTemplateEntity> Templates { get; set; }

    /// <summary>
    /// Represents user preferences for notifications
    /// </summary>
    public ICollection<UserNotificationPreferenceEntity> UserPreferences { get; set; }
}

/// <summary>
/// Event importance level (FR-1.2)
/// </summary>
public enum ImportanceLevel
{
    /// <summary>Represents a low level of importance</summary>
    Low = 0,

    /// <summary>Indicates a notification with medium importance</summary>
    Medium = 1,

    /// <summary>Represents a high level of importance</summary>
    High = 2,

    /// <summary>Represents a notification with the highest level of importance requiring immediate attention</summary>
    Critical = 3
}

/// <summary>
/// Notification delivery channels (FR-1)
/// </summary>
public class NotificationChannelEntity
{
    public int NotificationChannelId { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public bool RequiresVerification { get; set; } = false;
    public int? MaxRetryCount { get; set; }

    // Navigation properties
    public ICollection<NotificationTemplateEntity> Templates { get; set; }
    public ICollection<UserNotificationPreferenceEntity> UserPreferences { get; set; }
    public ICollection<NotificationDeliveryEntity> Deliveries { get; set; }
}

/// <summary>
/// Notification template for different channels (FR-2.2.1)
/// </summary>
public class NotificationTemplateEntity
{
    /// <summary>
    /// Represents the unique identifier for the notification template
    /// </summary>
    public int NotificationTemplateId { get; set; }

    /// <summary>
    /// Represents the unique identifier for the notification type
    /// </summary>
    public int NotificationTypeId { get; set; }

    /// <summary>
    /// Defines the subject template for the notification
    /// </summary>
    public string SubjectTemplate { get; set; }

    /// <summary>
    /// Contains the message text for the notification
    /// </summary>
    public string MessageTemplate { get; set; }

    /// <summary>
    /// Indicates whether the notification template is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public NotificationTypeEntity NotificationType { get; set; }
    public ICollection<NotificationTemplateChannelEntity> NotificationTemplateChannels { get; set; }
    public ICollection<NotificationTemplateUserEntity> NotificationTemplateUsers { get; set; }
}

public class NotificationTemplateChannelEntity
{
    public int NotificationTemplateChannelId { get; set; }
    public int NotificationTemplateId { get; set; }
    public int NotificationChannelId { get; set; }
    public NotificationTemplateEntity NotificationTemplate { get; set; }
    public NotificationChannelEntity NotificationChannel { get; set; }
}

/// <summary>
/// Represents a mapping between a notification template and a user
/// </summary>
public class NotificationTemplateUserEntity
{
    public int NotificationTemplateUserId { get; set; }
    public int NotificationTemplateId { get; set; }
    public int UserId { get; set; }
}

/// <summary>
/// User notification settings (FR-2)
/// </summary>
public class UserNotificationPreferenceEntity
{
    public int UserNotificationPreferenceId { get; set; }
    public int UserId { get; set; }
    public int NotificationTypeId { get; set; }
    public int NotificationChannelId { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Navigation properties
    public NotificationTypeEntity NotificationType { get; set; }
    public NotificationChannelEntity NotificationChannel { get; set; }
}

/// <summary>
/// Notification/event (FR-2.1.1, FR-3)
/// </summary>
public class NotificationEntity
{
    public long NotificationId { get; set; }
    public int NotificationTypeId { get; set; }
    public ImportanceLevel Importance { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string DataJson { get; set; }

    // Navigation properties
    public NotificationTypeEntity NotificationType { get; set; }
    public List<NotificationUserEntity> NotificationUsers { get; set; }
}

/// <summary>
/// Entity describing which users should be notified about an event from Notification
/// </summary>
public class NotificationUserEntity
{
    public long NotificationUserId { get; set; }
    public long NotificationId { get; set; }
    public int UserId { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public NotificationEntity Notification { get; set; }
    public ICollection<NotificationDeliveryEntity> Deliveries { get; set; }
}

/// <summary>
/// Notification delivery history by channels (FR-3)
/// </summary>
public class NotificationDeliveryEntity
{
    public long NotificationDeliveryId { get; set; }
    public long NotificationUserId { get; set; }
    public int UserId { get; set; }
    public int NotificationChannelId { get; set; }
    public DeliveryStatus Status { get; set; }
    public string? ExternalId { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }

    // Navigation properties
    public NotificationUserEntity NotificationUser { get; set; }
    public NotificationChannelEntity NotificationChannel { get; set; }
}

/// <summary>
/// Delivery status
/// </summary>
public enum DeliveryStatus
{
    Pending = 0,
    Processing = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Cancelled = 5
}