using Microsoft.EntityFrameworkCore;
using Oip.Data.Repositories;

namespace Oip.Discussions.Data.Repositories;

/// <summary>
/// Repository for managing ReactionEntity entities
/// </summary>
/// <param name="context">The database context</param>
public class ReactionRepository(DiscussionsDbContext context) 
    : BaseRepository<ReactionEntity, long>(context)
{
    /// <summary>
    /// Gets reactions by comment ID
    /// </summary>
    /// <param name="commentId">Comment identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of reactions for the comment</returns>
    public virtual async Task<List<ReactionEntity>> GetByCommentIdAsync(
        long commentId, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.CommentId == commentId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets reaction by comment ID and user ID
    /// </summary>
    /// <param name="commentId">Comment identifier</param>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reaction if found; otherwise, null</returns>
    public virtual async Task<ReactionEntity?> GetByCommentAndUserAsync(
        long commentId, 
        long userId, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Checks if user has reacted to a comment
    /// </summary>
    /// <param name="commentId">Comment identifier</param>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has reacted; otherwise, false</returns>
    public virtual async Task<bool> UserHasReactedAsync(
        long commentId, 
        long userId, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(r => r.CommentId == commentId && r.UserId == userId, cancellationToken);
    }
}
