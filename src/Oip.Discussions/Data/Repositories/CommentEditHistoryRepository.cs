using Microsoft.EntityFrameworkCore;
using Oip.Data.Repositories;

namespace Oip.Discussions.Data.Repositories;

/// <summary>
/// Repository for managing CommentEditHistoryEntity entities
/// </summary>
/// <param name="context">The database context</param>
public class CommentEditHistoryRepository(DiscussionsDbContext context) 
    : BaseRepository<CommentEditHistoryEntity, long>(context)
{
    /// <summary>
    /// Gets edit history by comment ID
    /// </summary>
    /// <param name="commentId">Comment identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of edit history records for the comment</returns>
    public virtual async Task<List<CommentEditHistoryEntity>> GetByCommentIdAsync(
        long commentId, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(h => h.CommentId == commentId)
            .OrderByDescending(h => h.EditedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets edit history by user ID
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of edit history records by the user</returns>
    public virtual async Task<List<CommentEditHistoryEntity>> GetByUserAsync(
        long userId, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(h => h.EditedByUserId == userId)
            .Include(h => h.Comment)
            .OrderByDescending(h => h.EditedAt)
            .ToListAsync(cancellationToken);
    }
}