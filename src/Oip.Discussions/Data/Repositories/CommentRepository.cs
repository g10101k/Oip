using Microsoft.EntityFrameworkCore;
using Oip.Data.Repositories;

namespace Oip.Discussions.Data.Repositories;

/// <summary>
/// Repository for managing CommentEntity entities
/// </summary>
/// <param name="context">The database context</param>
public class CommentRepository(DiscussionsDbContext context) : BaseRepository<CommentEntity, long>(context)
{
    /// <summary>
    /// Gets comments by object type and object ID with related data
    /// </summary>
    /// <param name="objectTypeId">Object type identifier</param>
    /// <param name="objectId">Object identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of comments with related data</returns>
    public virtual async Task<List<CommentEntity>> GetByObjectAsync(long objectTypeId, long objectId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Attachments)
            .Include(c => c.Reactions)
            .Include(c => c.Mentions)
            .Include(c => c.EditHistory)
            .Where(c => c.ObjectTypeId == objectTypeId && c.ObjectId == objectId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets comment with all related data by ID
    /// </summary>
    /// <param name="id">Comment identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comment with related data if found; otherwise, null</returns>
    public virtual async Task<CommentEntity?> GetByIdWithDetailsAsync(long id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Attachments)
            .Include(c => c.Reactions)
            .Include(c => c.Mentions)
            .Include(c => c.EditHistory)
            .FirstOrDefaultAsync(c => c.CommentId == id && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Soft delete comment by setting IsDeleted flag and DeletedAt timestamp
    /// </summary>
    /// <param name="id">Comment identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual async Task SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var comment = await GetByIdAsync(id, cancellationToken);
        if (comment != null)
        {
            comment.IsDeleted = true;
            comment.DeletedAt = DateTimeOffset.UtcNow;
            await UpdateAsync(comment, cancellationToken);
        }
    }

    /// <summary>
    /// Gets active (non-deleted) comments by user ID
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's active comments</returns>
    public virtual async Task<List<CommentEntity>> GetByUserAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}