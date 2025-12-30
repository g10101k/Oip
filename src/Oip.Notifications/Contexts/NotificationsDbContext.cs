using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Oip.Data.Contexts;
using Oip.Data.Extensions;

namespace Oip.Notifications.Contexts;

/// <summary>
/// Базовый репозиторий с общими методами CRUD
/// </summary>
public abstract class BaseRepository<TEntity, TKey>(NotificationsDbContext context)
    where TEntity : class
{
    /// <summary>
    /// Represents the set of entities in the database for a given type
    /// </summary>
    protected DbSet<TEntity> DbSet => context.Set<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id!], cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            DbSet.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await DbSet.CountAsync(cancellationToken);

        return await DbSet.CountAsync(predicate, cancellationToken);
    }
}

public class NotificationTypeRepository(NotificationsDbContext context)
    : BaseRepository<NotificationTypeEntity, int>(context)
{
    public async Task<NotificationTypeEntity?> GetByNameAsync(string name,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Name == name, cancellationToken);
    }

    public async Task<List<NotificationTypeEntity>> GetByScopeAsync(string scope,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => e.Scope == scope).ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationTypeEntity>> GetWithTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Templates)
            .ToListAsync(cancellationToken);
    }
}

public class NotificationChannelRepository(NotificationsDbContext context)
    : BaseRepository<NotificationChannelEntity, int>(context)
{
    public async Task<NotificationChannelEntity?> GetByNameAsync(string name,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Name == name, cancellationToken);
    }

    public async Task<List<NotificationChannelEntity>> GetActiveChannelsAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => e.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationChannelEntity>> GetChannelsRequiringVerificationAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => e.RequiresVerification).ToListAsync(cancellationToken);
    }
}

public class NotificationTemplateRepository(NotificationsDbContext context)
    : BaseRepository<NotificationTemplateEntity, int>(context)
{
    public async Task<List<NotificationTemplateEntity>> GetActiveTemplatesByTypeAsync(
        int notificationTypeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.NotificationTypeId == notificationTypeId && e.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificationTemplateEntity?> GetWithChannelsAsync(int id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.NotificationTemplateChannels)
            .ThenInclude(tc => tc.NotificationChannel)
            .Include(e => e.NotificationType)
            .FirstOrDefaultAsync(e => e.NotificationTemplateId == id, cancellationToken);
    }

    public async Task<List<NotificationTemplateEntity>> GetByTypeWithChannelsAsync(
        int notificationTypeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.NotificationTemplateChannels)
            .ThenInclude(tc => tc.NotificationChannel)
            .Where(e => e.NotificationTypeId == notificationTypeId && e.IsActive)
            .ToListAsync(cancellationToken);
    }
}

public class UserNotificationPreferenceRepository(NotificationsDbContext context)
    : BaseRepository<UserNotificationPreferenceEntity, int>(context)
{
    private readonly NotificationsDbContext _context = context;

    public async Task<List<UserNotificationPreferenceEntity>> GetByUserIdAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.NotificationType)
            .Include(p => p.NotificationChannel)
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserNotificationPreferenceEntity>> GetByUserAndTypeAsync(
        int userId, int notificationTypeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.NotificationChannel)
            .Where(p => p.UserId == userId && p.NotificationTypeId == notificationTypeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserNotificationPreferenceEntity?> GetPreferenceAsync(
        int userId, int notificationTypeId, int notificationChannelId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.NotificationType)
            .Include(p => p.NotificationChannel)
            .FirstOrDefaultAsync(p => p.UserId == userId &&
                                      p.NotificationTypeId == notificationTypeId &&
                                      p.NotificationChannelId == notificationChannelId,
                cancellationToken);
    }

    public async Task<UserNotificationPreferenceEntity> UpsertPreferenceAsync(
        UserNotificationPreferenceEntity preference, CancellationToken cancellationToken = default)
    {
        var existing = await GetPreferenceAsync(
            preference.UserId,
            preference.NotificationTypeId,
            preference.NotificationChannelId,
            cancellationToken);

        if (existing != null)
        {
            existing.IsEnabled = preference.IsEnabled;
            await UpdateAsync(existing, cancellationToken);
            return existing;
        }
        else
        {
            await AddAsync(preference, cancellationToken);
            return preference;
        }
    }

    public async Task DeleteByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var preferences = await GetByUserIdAsync(userId, cancellationToken);
        DbSet.RemoveRange(preferences);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class NotificationRepository(NotificationsDbContext context)
    : BaseRepository<NotificationEntity, long>(context)
{
    public async Task<List<NotificationEntity>> GetByTypeAsync(int notificationTypeId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.NotificationTypeId == notificationTypeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationEntity>> GetByImportanceAsync(ImportanceLevel importance,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.Importance == importance)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationEntity>> GetCreatedAfterAsync(DateTimeOffset date,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.CreatedAt >= date)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificationEntity?> GetWithUsersAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(n => n.NotificationUsers)
            .Include(n => n.NotificationType)
            .FirstOrDefaultAsync(n => n.NotificationId == id, cancellationToken);
    }

    public async Task<List<NotificationEntity>> GetByUserIdAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(n => n.NotificationType)
            .Include(n => n.NotificationUsers)
            .Where(n => n.NotificationUsers.Any(u => u.UserId == userId))
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

public class NotificationDeliveryRepository(NotificationsDbContext context)
    : BaseRepository<NotificationDeliveryEntity, long>(context)
{
    public async Task<List<NotificationDeliveryEntity>> GetByStatusAsync(DeliveryStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Include(d => d.NotificationChannel)
            .Where(d => d.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationDeliveryEntity>> GetByUserIdAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Include(d => d.NotificationChannel)
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationDeliveryEntity>> GetByChannelIdAsync(int channelId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Where(d => d.NotificationChannelId == channelId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationDeliveryEntity>> GetForRetryAsync(int maxRetryCount,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Include(d => d.NotificationChannel)
            .Where(d => d.Status == DeliveryStatus.Failed &&
                        d.RetryCount < maxRetryCount &&
                        d.NotificationChannel.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificationDeliveryEntity?> GetByExternalIdAsync(string externalId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Include(d => d.NotificationChannel)
            .FirstOrDefaultAsync(d => d.ExternalId == externalId, cancellationToken);
    }

    public async Task UpdateStatusAsync(long deliveryId, DeliveryStatus status, string? externalId = null,
        string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        var delivery = await GetByIdAsync(deliveryId, cancellationToken);
        if (delivery == null)
            throw new ArgumentException($"Delivery with id {deliveryId} not found");

        delivery.Status = status;
        delivery.ExternalId = externalId ?? delivery.ExternalId;
        delivery.ErrorMessage = errorMessage ?? delivery.ErrorMessage;

        if (status == DeliveryStatus.Sent)
            delivery.SentAt = DateTimeOffset.UtcNow;
        else if (status == DeliveryStatus.Delivered)
            delivery.DeliveredAt = DateTimeOffset.UtcNow;

        if (status == DeliveryStatus.Failed)
            delivery.RetryCount++;

        await UpdateAsync(delivery, cancellationToken);
    }
}

/// <summary>
/// Represents the SQL Server database context for user-related entities.
/// </summary>
public class NotificationsDbContextSqlServer(DbContextOptions<NotificationsDbContext> options, bool designTime = true)
    : NotificationsDbContext(options, designTime);

/// <summary>
/// Represents the PostgreSQL database context for user-related entities.
/// </summary>
public class NotificationsDbContextPostgres(
    DbContextOptions<NotificationsDbContext> options,
    bool designTime = true)
    : NotificationsDbContext(options, designTime);

/// <summary>
/// Database context for notification entities
/// </summary>
public class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options, bool designTime = false)
    : DbContext(options)
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

        modelBuilder.ApplyConfiguration(new NotificationTypeEntityConfiguration(Database, designTime));
        modelBuilder.ApplyConfiguration(new NotificationChannelEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationTemplateEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationTemplateChannelEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationTemplateUserEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new UserNotificationPreferenceEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationUserEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new NotificationDeliveryEntityConfiguration(Database));

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
/// Configures database mapping for NotificationTypeEntity
/// </summary>
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

/// <summary>
/// Configures database mapping for NotificationChannelEntity
/// </summary>
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

/// <summary>
/// Configures database mapping for NotificationTemplateEntity
/// </summary>
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

/// <summary>
/// Configures database mapping for NotificationTemplateChannelEntity
/// </summary>
public class NotificationTemplateChannelEntityConfiguration(DatabaseFacade database)
    : IEntityTypeConfiguration<NotificationTemplateChannelEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationTemplateChannelEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(database, NotificationsDbContext.SchemaName);

        builder.HasKey(e => e.NotificationTemplateChannelId);

        builder.Property(e => e.NotificationTemplateChannelId)
            .ValueGeneratedOnAdd();

        // Composite unique index to prevent duplicate relationships
        builder.HasIndex(e => new { e.NotificationTemplateId, e.NotificationChannelId })
            .IsUnique();
    }
}

/// <summary>
/// Configures database mapping for NotificationTemplateUserEntity
/// </summary>
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

/// <summary>
/// Configures database mapping for UserNotificationPreferenceEntity
/// </summary>
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

/// <summary>
/// Configures database mapping for NotificationEntity
/// </summary>
public class NotificationEntityConfiguration(DatabaseFacade database)
    : IEntityTypeConfiguration<NotificationEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        // Set table with schema for notifications
        builder.SetTableWithSchema(database, NotificationsDbContext.SchemaName);

        builder.HasKey(e => e.NotificationId);

        builder.Property(e => e.NotificationId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Importance)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

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

/// <summary>
/// Configures database mapping for NotificationUserEntity
/// </summary>
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

/// <summary>
/// Configures database mapping for NotificationDeliveryEntity
/// </summary>
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
/// Type of notification within the system
/// </summary>
public class NotificationTypeEntity
{
    /// <summary>
    /// Unique identifier for the notification type
    /// </summary>
    public int NotificationTypeId { get; set; }

    /// <summary>
    /// Name of the notification type
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Detailed explanation of the notification type
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Scope of notification type (global, appName, feature)
    /// </summary>
    public required string Scope { get; set; }

    /// <summary>
    /// Collection of notification templates associated with this notification type
    /// </summary>
    public List<NotificationTemplateEntity> Templates { get; set; } = new();

    /// <summary>
    /// User preferences for notifications of this type
    /// </summary>
    public List<UserNotificationPreferenceEntity> UserPreferences { get; set; } = new();
}

/// <summary>
/// Event importance levels
/// </summary>
public enum ImportanceLevel
{
    /// <summary>Low level of importance</summary>
    Low = 0,

    /// <summary>Notification with medium importance</summary>
    Medium = 1,

    /// <summary>High level of importance</summary>
    High = 2,

    /// <summary>Highest level of importance requiring immediate attention</summary>
    Critical = 3
}

/// <summary>
/// Notification delivery channels
/// </summary>
public class NotificationChannelEntity
{
    /// <summary>
    /// Unique identifier for the notification channel
    /// </summary>
    public int NotificationChannelId { get; set; }

    /// <summary>
    /// Name of the notification channel
    /// </summary>
    public required string Name { get; set; } = null!;

    /// <summary>
    /// Whether the channel is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the channel requires user verification
    /// </summary>
    public bool RequiresVerification { get; set; } = false;

    /// <summary>
    /// Maximum number of delivery retry attempts
    /// </summary>
    public int? MaxRetryCount { get; set; }

    /// <summary>
    /// Collection of notification templates associated with this channel
    /// </summary>
    public List<NotificationTemplateEntity> Templates { get; set; } = new();

    /// <summary>
    /// User-specific preferences for this channel
    /// </summary>
    public List<UserNotificationPreferenceEntity> UserPreferences { get; set; } = new();

    /// <summary>
    /// List of deliveries associated with this channel
    /// </summary>
    public List<NotificationDeliveryEntity> Deliveries { get; set; } = new();
}

/// <summary>
/// Notification template for different channels
/// </summary>
public class NotificationTemplateEntity
{
    /// <summary>
    /// Unique identifier for the notification template
    /// </summary>
    public int NotificationTemplateId { get; set; }

    /// <summary>
    /// Unique identifier for the notification type
    /// </summary>
    public int NotificationTypeId { get; set; }

    /// <summary>
    /// Subject template for the notification
    /// </summary>
    public string SubjectTemplate { get; set; } = null!;

    /// <summary>
    /// Message template for the notification
    /// </summary>
    public string MessageTemplate { get; set; } = null!;

    /// <summary>
    /// Whether the notification template is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Associated notification type
    /// </summary>
    public NotificationTypeEntity NotificationType { get; set; } = null!;

    /// <summary>
    /// Channels associated with this template
    /// </summary>
    public List<NotificationTemplateChannelEntity> NotificationTemplateChannels { get; set; } = new();

    /// <summary>
    /// Users associated with this template
    /// </summary>
    public List<NotificationTemplateUserEntity> NotificationTemplateUsers { get; set; } = new();
}

/// <summary>
/// Association between a notification template and a notification channel
/// </summary>
public class NotificationTemplateChannelEntity
{
    /// <summary>
    /// Unique identifier for the template-channel association
    /// </summary>
    public int NotificationTemplateChannelId { get; set; }

    /// <summary>
    /// Notification template identifier
    /// </summary>
    public int NotificationTemplateId { get; set; }

    /// <summary>
    /// Notification channel identifier
    /// </summary>
    public int NotificationChannelId { get; set; }

    /// <summary>
    /// Associated notification template
    /// </summary>
    public NotificationTemplateEntity NotificationTemplate { get; set; } = null!;

    /// <summary>
    /// Associated notification channel
    /// </summary>
    public NotificationChannelEntity NotificationChannel { get; set; } = null!;
}

/// <summary>
/// Mapping between a notification template and a user
/// </summary>
public class NotificationTemplateUserEntity
{
    /// <summary>
    /// Unique identifier for the template-user mapping
    /// </summary>
    public int NotificationTemplateUserId { get; set; }

    /// <summary>
    /// Notification template identifier
    /// </summary>
    public int NotificationTemplateId { get; set; }

    /// <summary>
    /// User identifier
    /// </summary>
    public int UserId { get; set; }
}

/// <summary>
/// User notification preferences
/// </summary>
public class UserNotificationPreferenceEntity
{
    /// <summary>
    /// Unique identifier for the user notification preference
    /// </summary>
    public int UserNotificationPreferenceId { get; set; }

    /// <summary>
    /// User identifier
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Notification type identifier
    /// </summary>
    public int NotificationTypeId { get; set; }

    /// <summary>
    /// Notification channel identifier
    /// </summary>
    public int NotificationChannelId { get; set; }

    /// <summary>
    /// Whether notifications are enabled for this preference
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Associated notification type
    /// </summary>
    public NotificationTypeEntity NotificationType { get; set; } = null!;

    /// <summary>
    /// Associated notification channel
    /// </summary>
    public NotificationChannelEntity NotificationChannel { get; set; } = null!;
}

/// <summary>
/// Notification/event
/// </summary>
public class NotificationEntity
{
    /// <summary>
    /// Unique identifier for the notification
    /// </summary>
    public long NotificationId { get; set; }

    /// <summary>
    /// Notification type identifier
    /// </summary>
    public int NotificationTypeId { get; set; }

    /// <summary>
    /// Importance level of the notification
    /// </summary>
    public ImportanceLevel Importance { get; set; }

    /// <summary>
    /// Creation timestamp of the notification
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// JSON data associated with the notification
    /// </summary>
    public string? DataJson { get; set; }

    /// <summary>
    /// Associated notification type
    /// </summary>
    public NotificationTypeEntity NotificationType { get; set; } = null!;

    /// <summary>
    /// Users associated with this notification
    /// </summary>
    public List<NotificationUserEntity> NotificationUsers { get; set; } = new();
}

/// <summary>
/// Users who should be notified about an event
/// </summary>
public class NotificationUserEntity
{
    /// <summary>
    /// Unique identifier for the notification user
    /// </summary>
    public long NotificationUserId { get; set; }

    /// <summary>
    /// Notification identifier
    /// </summary>
    public long NotificationId { get; set; }

    /// <summary>
    /// User identifier
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Subject of the notification for this user
    /// </summary>
    public string Subject { get; set; } = null!;

    /// <summary>
    /// Message of the notification for this user
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Associated notification
    /// </summary>
    public NotificationEntity Notification { get; set; } = null!;

    /// <summary>
    /// Delivery attempts for this notification user
    /// </summary>
    public List<NotificationDeliveryEntity> Deliveries { get; set; } = new();
}

/// <summary>
/// Notification delivery history by channels
/// </summary>
public class NotificationDeliveryEntity
{
    /// <summary>
    /// Unique identifier for the notification delivery
    /// </summary>
    public long NotificationDeliveryId { get; set; }

    /// <summary>
    /// Notification user identifier
    /// </summary>
    public long NotificationUserId { get; set; }

    /// <summary>
    /// User identifier
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Notification channel identifier
    /// </summary>
    public int NotificationChannelId { get; set; }

    /// <summary>
    /// Delivery status
    /// </summary>
    public DeliveryStatus Status { get; set; }

    /// <summary>
    /// External identifier from the delivery service
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Error message if delivery failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of delivery retry attempts
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Creation timestamp of the delivery record
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the notification was sent
    /// </summary>
    public DateTimeOffset? SentAt { get; set; }

    /// <summary>
    /// Timestamp when the notification was delivered
    /// </summary>
    public DateTimeOffset? DeliveredAt { get; set; }

    /// <summary>
    /// Associated notification user
    /// </summary>
    public NotificationUserEntity NotificationUser { get; set; } = null!;

    /// <summary>
    /// Associated notification channel
    /// </summary>
    public NotificationChannelEntity NotificationChannel { get; set; } = null!;
}

/// <summary>
/// Delivery status values
/// </summary>
public enum DeliveryStatus
{
    /// <summary>Delivery is pending</summary>
    Pending = 0,

    /// <summary>Delivery is being processed</summary>
    Processing = 1,

    /// <summary>Notification has been sent</summary>
    Sent = 2,

    /// <summary>Notification has been delivered</summary>
    Delivered = 3,

    /// <summary>Delivery failed</summary>
    Failed = 4,

    /// <summary>Delivery was cancelled</summary>
    Cancelled = 5
}