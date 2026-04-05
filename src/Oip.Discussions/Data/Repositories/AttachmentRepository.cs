using Microsoft.EntityFrameworkCore;
using Oip.Data.Repositories;

namespace Oip.Discussions.Data.Repositories;

/// <summary>
/// Repository for managing AttachmentEntity entities
/// </summary>
/// <param name="context">The database context</param>
public class AttachmentRepository(DiscussionsDbContext context) 
    : BaseRepository<AttachmentEntity, long>(context)
{
    /// <summary>
    /// Gets attachments by comment ID
    /// </summary>
    /// <param name="commentId">Comment identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of attachments for the comment</returns>
    public virtual async Task<List<AttachmentEntity>> GetByCommentIdAsync(
        long commentId, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.CommentId == commentId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets attachment by storage file ID
    /// </summary>
    /// <param name="storageFileId">Storage file identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Attachment if found; otherwise, null</returns>
    public virtual async Task<AttachmentEntity?> GetByStorageFileIdAsync(
        Guid storageFileId, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(a => a.StorageFileId == storageFileId, cancellationToken);
    }
}
