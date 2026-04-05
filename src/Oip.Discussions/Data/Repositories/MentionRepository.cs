using Microsoft.EntityFrameworkCore;
using Oip.Data.Repositories;

namespace Oip.Discussions.Data.Repositories;

/// <summary>
/// Repository for managing MentionEntity entities
/// </summary>
/// <param name="context">The database context</param>
public class MentionRepository(DiscussionsDbContext context) 
    : BaseRepository<MentionEntity, long>(context)
{
    /// <summary>
    /// Gets mentions by comment ID
    /// </summary>
    /// <param name="commentId">Comment identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of mentions in the comment</returns>
    public virtual async Task<List<MentionEntity>> GetByCommentIdAsync(
        long commentId, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => m.CommentId == commentId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets mentions by mentioned user ID
    /// </summary>
    /// <param name="mentionedUserId">Mentioned user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of mentions for the user</returns>
    public virtual async Task<List<MentionEntity>> GetByMentionedUserAsync(
        long mentionedUserId, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => m.MentionedUserId == mentionedUserId)
            .Include(m => m.Comment)
            .ToListAsync(cancellationToken);
    }
}
