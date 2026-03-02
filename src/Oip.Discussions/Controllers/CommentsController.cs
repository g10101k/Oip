using Microsoft.AspNetCore.Mvc;
using Oip.Discussions.Data;
using Oip.Discussions.Data.Repositories;

namespace Oip.Discussions.Controllers;

/// <summary>
/// Provides API endpoints for managing comments.
/// </summary>
[ApiController]
[Route("api/comments")]
public class CommentsController(CommentRepository commentRepository) : ControllerBase
{
    /// <summary>
    /// Gets comments by object type and object ID
    /// </summary>
    /// <param name="objectTypeId">Object type identifier</param>
    /// <param name="objectId">Object identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of comments</returns>
    [HttpGet("get-by-object")]
    public async Task<ActionResult<IEnumerable<CommentEntity>>> GetByObjectAsync(long objectTypeId, long objectId,
        CancellationToken cancellationToken = default)
    {
        var comments = await commentRepository.GetByObjectAsync(objectTypeId, objectId, cancellationToken);
        return Ok(comments);
    }

    /// <summary>
    /// Gets comment by ID with all details
    /// </summary>
    /// <param name="id">Comment identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comment with details</returns>
    [HttpGet("get-by-id")]
    public async Task<ActionResult<CommentEntity>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (comment == null)
        {
            return NotFound();
        }

        return Ok(comment);
    }

    /// <summary>
    /// Creates a new comment
    /// </summary>
    /// <param name="comment">Comment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created comment</returns>
    [HttpPost]
    public async Task<ActionResult<CommentEntity>> CreateAsync(CommentEntity comment,
        CancellationToken cancellationToken = default)
    {
        var createdComment = await commentRepository.AddAsync(comment, cancellationToken);
        return Ok(createdComment);
    }

    /// <summary>
    /// Updates an existing comment
    /// </summary>
    /// <param name="id">Comment identifier</param>
    /// <param name="comment">Updated comment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated comment</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<CommentEntity>> UpdateAsync(long id, CommentEntity comment,
        CancellationToken cancellationToken = default)
    {
        if (id != comment.CommentId)
        {
            return BadRequest("Comment ID mismatch");
        }

        var existingComment = await commentRepository.GetByIdAsync(id, cancellationToken);
        if (existingComment == null)
        {
            return NotFound();
        }

        var updatedComment = await commentRepository.UpdateAsync(comment, cancellationToken);
        return Ok(updatedComment);
    }

    /// <summary>
    /// Soft deletes a comment
    /// </summary>
    /// <param name="id">Comment identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await commentRepository.SoftDeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets comments by user ID
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's comments</returns>
    [HttpGet("by-user/{userId}")]
    public async Task<ActionResult<IEnumerable<CommentEntity>>> GetByUserAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        var comments = await commentRepository.GetByUserAsync(userId, cancellationToken);
        return Ok(comments);
    }
}