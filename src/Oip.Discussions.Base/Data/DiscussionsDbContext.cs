using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Oip.Data.Contexts;
using Oip.Data.Extensions;
using Oip.Discussions.Data.EntityConfigurations;

namespace Oip.Discussions.Data;

/// <summary>
/// Database context for managing discussions data including comments, attachments, reactions, mentions, and edit history.
/// </summary>
/// <param name="options">Configuration options for this context</param>
/// <param name="designTime">Indicates if the context is being used during design time</param>
public class DiscussionsDbContext(DbContextOptions<DiscussionsDbContext> options, bool designTime = false)
    : DbContext(options), IDataProtectionKeyContext
{
    /// <summary>
    /// The name of the database schema used for discussions-related entities.
    /// </summary>
    public const string SchemaName = "discussions";

    /// <summary>
    /// The name of the table used to store migration history.
    /// </summary>
    public const string MigrationHistoryTableName = "__MigrationHistory";

    /// <summary>
    /// Comment entities for tracking user feedback and discussions
    /// </summary>
    public DbSet<CommentEntity> Comments { get; set; }

    /// <summary>
    /// Collection of file attachments associated with discussion comments
    /// </summary>
    public DbSet<AttachmentEntity> Attachments { get; set; }

    /// <summary>
    /// Collection of reaction entities associated with comments in the discussions database
    /// </summary>
    public DbSet<ReactionEntity> Reactions => Set<ReactionEntity>();

    /// <summary>
    /// Collection of mention entities stored in the discussions database
    /// </summary>
    public DbSet<MentionEntity> Mentions => Set<MentionEntity>();

    /// <summary>
    /// History of edits made to comments in the discussion system
    /// </summary>
    public DbSet<CommentEditHistoryEntity> CommentEditHistory => Set<CommentEditHistoryEntity>();

    /// <summary>
    /// Data protection keys used for cryptographic operations
    /// </summary>
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

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
                BaseContextMigrationAssembly<DiscussionsDbContextSqlServer, DiscussionsDbContextPostgres>>();

        if (!optionsBuilder.IsConfigured)
            throw new InvalidOperationException("OnConfiguring error");
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CommentEntityConfiguration(Database, designTime, SchemaName));
        modelBuilder.ApplyConfiguration(new AttachmentEntityConfiguration(Database, designTime, SchemaName));
        modelBuilder.ApplyConfiguration(new CommentEditHistoryEntityConfiguration(Database, designTime, SchemaName));
        modelBuilder.ApplyConfiguration(new ReactionEntityConfiguration(Database, designTime, SchemaName));
        modelBuilder.ApplyConfiguration(new MentionEntityConfiguration(Database, designTime, SchemaName));

        modelBuilder.ApplyXmlDocumentation(designTime);
    }
}

/// <summary>
/// Represents the SQL Server database context for user-related entities.
/// </summary>
public class DiscussionsDbContextSqlServer(
    DbContextOptions<DiscussionsDbContext> options,
    bool designTime = true)
    : DiscussionsDbContext(options, designTime);

/// <summary>
/// Represents the PostgreSQL database context for user-related entities.
/// </summary>
public class DiscussionsDbContextPostgres(
    DbContextOptions<DiscussionsDbContext> options,
    bool designTime = true)
    : DiscussionsDbContext(options, designTime);

/// <summary>
/// Represents a comment entity with content, metadata, and related associations.
/// </summary>
public class CommentEntity
{
    /// <summary>
    /// Unique identifier of the comment.
    /// </summary>
    public long CommentId { get; set; }

    /// <summary>
    /// Identifier of the object type associated with the comment.
    /// </summary>
    public long ObjectTypeId { get; set; }

    /// <summary>
    /// Identifier of the object associated with the comment.
    /// </summary>
    public long ObjectId { get; set; }

    /// <summary>
    /// Content of the comment.
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Identifier of the user who created the comment.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Date and time when the comment was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Date and time when the comment was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Date and time when the comment was deleted.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Indicates whether the comment is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Collection of attachments associated with the comment.
    /// </summary>
    public virtual ICollection<AttachmentEntity> Attachments { get; set; } = new HashSet<AttachmentEntity>();

    /// <summary>
    /// Collection of reactions associated with the comment.
    /// </summary>
    public virtual ICollection<ReactionEntity> Reactions { get; set; } = new HashSet<ReactionEntity>();

    /// <summary>
    /// Collection of mentions associated with the comment.
    /// </summary>
    public virtual ICollection<MentionEntity> Mentions { get; set; } = new HashSet<MentionEntity>();

    /// <summary>
    /// Collection of edit history records for the comment.
    /// </summary>
    public virtual ICollection<CommentEditHistoryEntity> EditHistory { get; set; } =
        new HashSet<CommentEditHistoryEntity>();
}

/// <summary>
/// Represents an attachment entity associated with a comment.
/// </summary>
public class AttachmentEntity
{
    /// <summary>
    /// Unique identifier of the attachment.
    /// </summary>
    public long AttachmentId { get; set; }

    /// <summary>
    /// Identifier of the comment that contains this attachment.
    /// </summary>
    public long CommentId { get; set; }

    /// <summary>
    /// Original name of the attached file.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// Physical path to the attached file in storage.
    /// </summary>
    public string FilePath { get; set; } = null!;

    /// <summary>
    /// MIME type or file extension of the attachment.
    /// </summary>
    public string FileType { get; set; } = null!;

    /// <summary>
    /// Size of the attached file in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Date and time when the attachment was uploaded.
    /// </summary>
    public DateTimeOffset UploadedAt { get; set; }

    /// <summary>
    /// Navigation property to the associated comment entity.
    /// </summary>
    public virtual CommentEntity Comment { get; set; } = null!;

    /// <summary>
    /// Unique identifier for the file in the storage system.
    /// </summary>
    public Guid StorageFileId { get; set; }
}

/// <summary>
/// Represents a reaction entity where a user reacts to a comment with an emoji.
/// </summary>
public class ReactionEntity
{
    /// <summary>
    /// Unique identifier of the reaction.
    /// </summary>
    public long ReactionId { get; set; }

    /// <summary>
    /// Identifier of the comment that was reacted to.
    /// </summary>
    public long CommentId { get; set; }

    /// <summary>
    /// Identifier of the user who created the reaction.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Emoji code representing the reaction type (e.g., Like, Heart, etc.).
    /// </summary>
    public string EmojiCode { get; set; } = null!;

    /// <summary>
    /// Timestamp when the reaction was created.
    /// </summary>
    public DateTimeOffset ReactedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Navigation property to the comment entity.
    /// </summary>
    public virtual CommentEntity Comment { get; set; } = null!;
}

/// <summary>
/// Represents a mention entity where a user is mentioned in a comment.
/// </summary>
public class MentionEntity
{
    /// <summary>
    /// Unique identifier of the mention.
    /// </summary>
    public long MentionId { get; set; }

    /// <summary>
    /// Identifier of the comment containing the mention.
    /// </summary>
    public long CommentId { get; set; }

    /// <summary>
    /// Identifier of the user who was mentioned.
    /// </summary>
    public long MentionedUserId { get; set; }

    /// <summary>
    /// Position of the mention within the comment text.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Date and time when the mention was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Navigation property to the comment containing this mention.
    /// </summary>
    public virtual CommentEntity Comment { get; set; } = null!;
}

/// <summary>
/// Represents the edit history of a comment, tracking changes made to comment content.
/// </summary>
public class CommentEditHistoryEntity
{
    /// <summary>
    /// Unique identifier for the comment edit history record.
    /// </summary>
    public long CommentEditHistoryId { get; set; }

    /// <summary>
    /// Identifier of the comment that was edited.
    /// </summary>
    public long CommentId { get; set; }

    /// <summary>
    /// The content of the comment before the edit.
    /// </summary>
    public string OldContent { get; set; } = null!;

    /// <summary>
    /// The content of the comment after the edit.
    /// </summary>
    public string NewContent { get; set; } = null!;

    /// <summary>
    /// Identifier of the user who performed the edit.
    /// </summary>
    public long EditedByUserId { get; set; }

    /// <summary>
    /// Date and time when the edit was performed.
    /// </summary>
    public DateTimeOffset EditedAt { get; set; }

    /// <summary>
    /// Navigation property to the associated comment entity.
    /// </summary>
    public virtual CommentEntity Comment { get; set; } = null!;
}