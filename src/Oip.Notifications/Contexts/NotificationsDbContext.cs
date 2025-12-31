using System.Linq.Expressions;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Oip.Data.Contexts;
using Oip.Data.EntityConfigurations;
using Oip.Data.Extensions;

namespace Oip.Notifications.Contexts;

/// <summary>
/// Base repository with common CRUD operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key</typeparam>
/// <param name="context">The database context</param>
public abstract class BaseRepository<TEntity, TKey>(NotificationsDbContext context)
    where TEntity : class
{
    /// <summary>
    /// Represents the set of entities in the database for a given type
    /// </summary>
    protected DbSet<TEntity> DbSet => context.Set<TEntity>();

    /// <summary>
    /// Retrieves an entity by its primary key
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity if found; otherwise, null</returns>
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id!], cancellationToken);
    }

    /// <summary>
    /// Retrieves all entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of all entities</returns>
    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Finds entities that match the specified predicate
    /// </summary>
    /// <param name="predicate">The condition to filter entities</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of matching entities</returns>
    public virtual async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new entity to the database
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes an entity by its primary key
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            DbSet.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Checks if any entity matches the specified predicate
    /// </summary>
    /// <param name="predicate">The condition to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any matching entity exists; otherwise, false</returns>
    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Counts entities, optionally filtered by a predicate
    /// </summary>
    /// <param name="predicate">The condition to filter entities; null to count all</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of matching entities</returns>
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await DbSet.CountAsync(cancellationToken);
        return await DbSet.CountAsync(predicate, cancellationToken);
    }
}

/// <summary>
/// Repository for managing notification types
/// </summary>
/// <param name="context">The database context</param>
public class NotificationTypeRepository(NotificationsDbContext context)
    : BaseRepository<NotificationTypeEntity, int>(context)
{
    /// <summary>
    /// Retrieves a notification type by its name
    /// </summary>
    /// <param name="name">The name of the notification type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The notification type if found; otherwise, null</returns>
    public async Task<NotificationTypeEntity?> GetByNameAsync(string name,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Name == name, cancellationToken);
    }

    /// <summary>
    /// Retrieves notification types by scope
    /// </summary>
    /// <param name="scope">The scope to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of notification types with the specified scope</returns>
    public async Task<List<NotificationTypeEntity>> GetByScopeAsync(string scope,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => e.Scope == scope).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves notification types with their associated templates
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of notification types with templates</returns>
    public async Task<List<NotificationTypeEntity>> GetWithTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Templates)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Repository for managing notification channels
/// </summary>
/// <param name="context">The database context</param>
public class NotificationChannelRepository(NotificationsDbContext context)
    : BaseRepository<NotificationChannelEntity, int>(context)
{
    /// <summary>
    /// Retrieves a notification channel by its name
    /// </summary>
    /// <param name="name">The name of the notification channel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The notification channel if found; otherwise, null</returns>
    public async Task<NotificationChannelEntity?> GetByNameAsync(string name,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Name == name, cancellationToken);
    }

    /// <summary>
    /// Retrieves all active notification channels
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of active notification channels</returns>
    public async Task<List<NotificationChannelEntity>> GetActiveChannelsAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => e.IsActive).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves notification channels that require verification
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of channels requiring verification</returns>
    public async Task<List<NotificationChannelEntity>> GetChannelsRequiringVerificationAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => e.RequiresVerification).ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Repository for managing notification templates
/// </summary>
/// <param name="context">The database context</param>
public class NotificationTemplateRepository(NotificationsDbContext context)
    : BaseRepository<NotificationTemplateEntity, int>(context)
{
    /// <summary>
    /// Retrieves active templates for a specific notification type
    /// </summary>
    /// <param name="notificationTypeId">The notification type identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of active templates for the specified type</returns>
    public async Task<List<NotificationTemplateEntity>> GetActiveTemplatesByTypeAsync(
        int notificationTypeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.NotificationTypeId == notificationTypeId && e.IsActive)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a template with its associated channels
    /// </summary>
    /// <param name="id">The template identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The template with channels if found; otherwise, null</returns>
    public async Task<NotificationTemplateEntity?> GetWithChannelsAsync(int id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.NotificationTemplateChannels)
            .ThenInclude(tc => tc.NotificationChannel)
            .Include(e => e.NotificationType)
            .FirstOrDefaultAsync(e => e.NotificationTemplateId == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves templates for a specific notification type with their associated channels
    /// </summary>
    /// <param name="notificationTypeId">The notification type identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of templates with channels for the specified type</returns>
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

/// <summary>
/// Repository for managing user notification preferences
/// </summary>
/// <param name="context">The database context</param>
public class UserNotificationPreferenceRepository(NotificationsDbContext context)
    : BaseRepository<UserNotificationPreferenceEntity, int>(context)
{
    private readonly NotificationsDbContext _context = context;

    /// <summary>
    /// Retrieves notification preferences for a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of preferences for the specified user</returns>
    public async Task<List<UserNotificationPreferenceEntity>> GetByUserIdAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.NotificationType)
            .Include(p => p.NotificationChannel)
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves notification preferences for a specific user and notification type
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="notificationTypeId">The notification type identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of preferences for the specified user and type</returns>
    public async Task<List<UserNotificationPreferenceEntity>> GetByUserAndTypeAsync(
        int userId, int notificationTypeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.NotificationChannel)
            .Where(p => p.UserId == userId && p.NotificationTypeId == notificationTypeId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a specific preference for a user, notification type, and channel
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="notificationTypeId">The notification type identifier</param>
    /// <param name="notificationChannelId">The notification channel identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The preference if found; otherwise, null</returns>
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

    /// <summary>
    /// Inserts or updates a notification preference
    /// </summary>
    /// <param name="preference">The preference to upsert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated or inserted preference</returns>
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

    /// <summary>
    /// Deletes all notification preferences for a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DeleteByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var preferences = await GetByUserIdAsync(userId, cancellationToken);
        DbSet.RemoveRange(preferences);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Repository for managing notifications
/// </summary>
/// <param name="context">The database context</param>
public class NotificationRepository(NotificationsDbContext context)
    : BaseRepository<NotificationEntity, long>(context)
{
    /// <summary>
    /// Retrieves notifications by type
    /// </summary>
    /// <param name="notificationTypeId">The notification type identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of notifications for the specified type</returns>
    public async Task<List<NotificationEntity>> GetByTypeAsync(int notificationTypeId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.NotificationTypeId == notificationTypeId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves notifications by importance level
    /// </summary>
    /// <param name="importance">The importance level</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of notifications with the specified importance</returns>
    public async Task<List<NotificationEntity>> GetByImportanceAsync(ImportanceLevel importance,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.Importance == importance)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves notifications created after a specific date
    /// </summary>
    /// <param name="date">The cutoff date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of notifications created after the specified date</returns>
    public async Task<List<NotificationEntity>> GetCreatedAfterAsync(DateTimeOffset date,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.CreatedAt >= date)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a notification with its associated users
    /// </summary>
    /// <param name="id">The notification identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The notification with users if found; otherwise, null</returns>
    public async Task<NotificationEntity?> GetWithUsersAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(n => n.NotificationUsers)
            .Include(n => n.NotificationType)
            .FirstOrDefaultAsync(n => n.NotificationId == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves notifications for a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of notifications for the specified user</returns>
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

/// <summary>
/// Repository for managing notification deliveries
/// </summary>
/// <param name="context">The database context</param>
public class NotificationDeliveryRepository(NotificationsDbContext context)
    : BaseRepository<NotificationDeliveryEntity, long>(context)
{
    /// <summary>
    /// Retrieves deliveries by status
    /// </summary>
    /// <param name="status">The delivery status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of deliveries with the specified status</returns>
    public async Task<List<NotificationDeliveryEntity>> GetByStatusAsync(DeliveryStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Include(d => d.NotificationChannel)
            .Where(d => d.Status == status)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves deliveries for a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of deliveries for the specified user</returns>
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

    /// <summary>
    /// Retrieves deliveries for a specific channel
    /// </summary>
    /// <param name="channelId">The channel identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of deliveries for the specified channel</returns>
    public async Task<List<NotificationDeliveryEntity>> GetByChannelIdAsync(int channelId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Where(d => d.NotificationChannelId == channelId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves failed deliveries eligible for retry
    /// </summary>
    /// <param name="maxRetryCount">The maximum retry count</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of deliveries eligible for retry</returns>
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

    /// <summary>
    /// Retrieves a delivery by its external identifier
    /// </summary>
    /// <param name="externalId">The external identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The delivery if found; otherwise, null</returns>
    public async Task<NotificationDeliveryEntity?> GetByExternalIdAsync(string externalId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.NotificationUser)
            .Include(d => d.NotificationChannel)
            .FirstOrDefaultAsync(d => d.ExternalId == externalId, cancellationToken);
    }

    /// <summary>
    /// Updates the status of a delivery
    /// </summary>
    /// <param name="deliveryId">The delivery identifier</param>
    /// <param name="status">The new status</param>
    /// <param name="externalId">The external identifier (optional)</param>
    /// <param name="errorMessage">The error message if applicable (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="ArgumentException">Thrown when delivery is not found</exception>
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
public class NotificationsDbContextPostgres(
    DbContextOptions<NotificationsDbContext> options,
    bool designTime = true)
    : NotificationsDbContext(options, designTime);

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
        modelBuilder.ApplyConfiguration(new NotificationTypeEntityConfiguration(Database, designTime));
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
/// <param name="database">The database facade</param>
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
/// <param name="database">The database facade</param>
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

/// <summary>
/// Configures database mapping for NotificationEntity
/// </summary>
/// <param name="database">The database facade</param>
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