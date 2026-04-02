using System.ComponentModel.DataAnnotations;

namespace Oip.Discussions.Contracts;

/// <summary>
/// Request for creating a new comment.
/// </summary>
public record CreateCommentRequest
{
    /// <summary>
    /// Related entity type identifier.
    /// </summary>
    public long ObjectTypeId { get; set; }

    /// <summary>
    /// Related entity identifier.
    /// </summary>
    public long ObjectId { get; set; }

    /// <summary>
    /// Raw markdown comment content.
    /// </summary>
    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Request for updating an existing comment.
/// </summary>
public record UpdateCommentRequest
{
    /// <summary>
    /// Raw markdown comment content.
    /// </summary>
    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Request for uploading an attachment to a comment.
/// </summary>
public record UploadAttachmentRequest
{
    /// <summary>
    /// Comment identifier.
    /// </summary>
    [Required]
    public long CommentId { get; set; }

    /// <summary>
    /// Attached file.
    /// </summary>
    [Required]
    public IFormFile File { get; set; } = null!;
}

/// <summary>
/// Request for adding or toggling a reaction for a comment.
/// </summary>
public record AddReactionRequest
{
    /// <summary>
    /// Comment identifier.
    /// </summary>
    public long CommentId { get; set; }

    /// <summary>
    /// Emoji code for the reaction.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string EmojiCode { get; set; } = string.Empty;
}

/// <summary>
/// Request for searching mention candidates.
/// </summary>
public record SearchMentionUsersRequest
{
    /// <summary>
    /// Search term.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Term { get; set; } = string.Empty;
}

/// <summary>
/// Comment response DTO.
/// </summary>
public record CommentDto
{
    /// <summary>
    /// Comment identifier.
    /// </summary>
    public long CommentId { get; set; }

    /// <summary>
    /// Related entity type identifier.
    /// </summary>
    public long ObjectTypeId { get; set; }

    /// <summary>
    /// Related entity identifier.
    /// </summary>
    public long ObjectId { get; set; }

    /// <summary>
    /// Raw markdown comment content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Author identifier.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Author display name.
    /// </summary>
    public string AuthorDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Author e-mail.
    /// </summary>
    public string AuthorEmail { get; set; } = string.Empty;

    /// <summary>
    /// Comment creation time.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Comment last update time.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Whether the comment was edited.
    /// </summary>
    public bool IsEdited { get; set; }

    /// <summary>
    /// Number of edit history entries.
    /// </summary>
    public int HistoryCount { get; set; }

    /// <summary>
    /// Whether the current user can edit this comment.
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Whether the current user can delete this comment.
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// Attachments for the comment.
    /// </summary>
    public List<AttachmentDto> Attachments { get; set; } = [];

    /// <summary>
    /// Reactions aggregated for the comment.
    /// </summary>
    public List<CommentReactionDto> Reactions { get; set; } = [];

    /// <summary>
    /// Mentions for the comment.
    /// </summary>
    public List<CommentMentionDto> Mentions { get; set; } = [];
}

/// <summary>
/// Comment edit history response DTO.
/// </summary>
public record CommentHistoryDto
{
    /// <summary>
    /// History entry identifier.
    /// </summary>
    public long CommentEditHistoryId { get; set; }

    /// <summary>
    /// Previous comment content.
    /// </summary>
    public string OldContent { get; set; } = string.Empty;

    /// <summary>
    /// Updated comment content.
    /// </summary>
    public string NewContent { get; set; } = string.Empty;

    /// <summary>
    /// Editor identifier.
    /// </summary>
    public long EditedByUserId { get; set; }

    /// <summary>
    /// Editor display name.
    /// </summary>
    public string EditedByDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Time of edit.
    /// </summary>
    public DateTimeOffset EditedAt { get; set; }
}

/// <summary>
/// Attachment response DTO.
/// </summary>
public record AttachmentDto
{
    /// <summary>
    /// Attachment identifier.
    /// </summary>
    public long AttachmentId { get; set; }

    /// <summary>
    /// Original file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME type of the file.
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Upload timestamp.
    /// </summary>
    public DateTimeOffset UploadedAt { get; set; }

    /// <summary>
    /// File storage identifier.
    /// </summary>
    public Guid StorageFileId { get; set; }

    /// <summary>
    /// Download URL for the file.
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;
}

/// <summary>
/// Aggregated reaction response DTO.
/// </summary>
public record CommentReactionDto
{
    /// <summary>
    /// Emoji code.
    /// </summary>
    public string EmojiCode { get; set; } = string.Empty;

    /// <summary>
    /// Number of reactions with the same emoji.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Whether the current user reacted with this emoji.
    /// </summary>
    public bool ReactedByCurrentUser { get; set; }
}

/// <summary>
/// Mention response DTO.
/// </summary>
public record CommentMentionDto
{
    /// <summary>
    /// Mentioned user identifier.
    /// </summary>
    public long MentionedUserId { get; set; }

    /// <summary>
    /// Display name of the mentioned user.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// E-mail of the mentioned user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Position within the markdown text.
    /// </summary>
    public int Position { get; set; }
}

/// <summary>
/// Mention candidate response DTO.
/// </summary>
public record MentionUserDto
{
    /// <summary>
    /// User identifier.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// E-mail.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
