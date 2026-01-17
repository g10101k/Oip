// Сущность комментария

public class CommentEntity
{
    public string ObjectType { get; set; }
    public string ObjectId { get; set; }
    public long CommentId { get; set; }
    public string Content { get; set; }
    public long UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Навигация к вложениям
    public virtual ICollection<AttachmentEntity> Attachments { get; set; }
    public virtual ICollection<ReactionEntity> Reactions { get; set; }
    public virtual ICollection<MentionEntity> Mentions { get; set; }
    public virtual ICollection<CommentEditHistoryEntity> EditHistory { get; set; }
}

// Сущность вложения
public class AttachmentEntity
{
    public Guid Id { get; set; }
    public long CommentId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string FileType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }

    // Навигация к комментарию
    public virtual CommentEntity Comment { get; set; }
    public Guid StorageFileId { get; set; }
}

// Сущность реакции
public class ReactionEntity
{
    public Guid Id { get; set; }
    public long CommentId { get; set; }
    public Guid UserId { get; set; }
    public string EmojiCode { get; set; } // или Type (Like, Heart, etc.)
    public DateTime ReactedAt { get; set; }
    public virtual CommentEntity Comment { get; set; }
}

// Сущность упоминания
public class MentionEntity
{
    public long Id { get; set; }
    public long CommentId { get; set; }
    public long MentionedUserId { get; set; }
    public int Position { get; set; } // Позиция в тексте
    public DateTime CreatedAt { get; set; }

    // Навигация
    public virtual CommentEntity Comment { get; set; }
}

// История редактирования комментария
public class CommentEditHistoryEntity
{
    public long Id { get; set; }
    public long CommentId { get; set; }
    public string OldContent { get; set; }
    public string NewContent { get; set; }
    public Guid EditedByUserId { get; set; }
    public DateTime EditedAt { get; set; }

    // Навигация
    public virtual CommentEntity Comment { get; set; }
}