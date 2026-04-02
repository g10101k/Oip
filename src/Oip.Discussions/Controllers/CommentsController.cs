using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Exceptions;
using Oip.Discussions.Contracts;
using Oip.Discussions.Services;

namespace Oip.Discussions.Controllers;

/// <summary>
/// Provides API endpoints for managing entity discussions.
/// </summary>
[ApiController]
[Authorize]
[Route("api/comments")]
public class CommentsController(CommentService commentService) : ControllerBase
{
    /// <summary>
    /// Gets comments by object type and object identifier.
    /// </summary>
    [HttpGet("get-by-object")]
    [ProducesResponseType<CommentDto[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> GetByObjectAsync(
        [FromQuery] long objectTypeId,
        [FromQuery] long objectId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var comments = await commentService.GetByObjectAsync(objectTypeId, objectId, skip, take, cancellationToken);
        return Ok(comments);
    }

    /// <summary>
    /// Gets a comment by identifier.
    /// </summary>
    [HttpGet("get-by-id")]
    [ProducesResponseType<CommentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> GetByIdAsync(
        [FromQuery] long id,
        CancellationToken cancellationToken = default)
    {
        var comment = await commentService.GetByIdAsync(id, cancellationToken);
        if (comment == null)
        {
            throw new NotFoundException($"Comment {id} was not found.");
        }

        return Ok(comment);
    }

    /// <summary>
    /// Creates a new comment
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType<CommentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CommentDto>> CreateAsync(
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var comment = await commentService.CreateAsync(request, cancellationToken);
        return Ok(comment);
    }

    /// <summary>
    /// Updates an existing comment.
    /// </summary>
    [HttpPut("update/{id}")]
    [ProducesResponseType<CommentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> UpdateAsync(
        long id,
        [FromBody] UpdateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var comment = await commentService.UpdateAsync(id, request, cancellationToken);
        if (comment == null)
        {
            throw new NotFoundException($"Comment {id} was not found.");
        }

        return Ok(comment);
    }

    /// <summary>
    /// Soft deletes a comment.
    /// </summary>
    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await commentService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Comment {id} was not found.");
        }

        return NoContent();
    }

    /// <summary>
    /// Gets edit history for a comment.
    /// </summary>
    [HttpGet("get-history/{id}")]
    [ProducesResponseType<CommentHistoryDto[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CommentHistoryDto>>> GetHistoryAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var history = await commentService.GetHistoryAsync(id, cancellationToken);
        if (history == null)
        {
            throw new NotFoundException($"Comment {id} was not found.");
        }

        return Ok(history);
    }

    /// <summary>
    /// Uploads an attachment for a comment.
    /// </summary>
    [HttpPost("upload-attachment")]
    [ProducesResponseType<AttachmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttachmentDto>> UploadAttachmentAsync(
        [FromForm] UploadAttachmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var attachment = await commentService.UploadAttachmentAsync(request, cancellationToken);
        return Ok(attachment);
    }

    /// <summary>
    /// Deletes an attachment.
    /// </summary>
    [HttpDelete("delete-attachment/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttachmentAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await commentService.DeleteAttachmentAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Attachment {id} was not found.");
        }

        return NoContent();
    }

    /// <summary>
    /// Downloads attachment content.
    /// </summary>
    [HttpGet("get-attachment-content/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttachmentContentAsync(long id, CancellationToken cancellationToken = default)
    {
        var content = await commentService.GetAttachmentContentAsync(id, cancellationToken);
        if (content == null)
        {
            throw new NotFoundException($"Attachment {id} was not found.");
        }

        return File(content.Content, content.ContentType, content.FileName);
    }

    /// <summary>
    /// Adds or toggles a reaction for a comment.
    /// </summary>
    [HttpPost("add-reaction")]
    [ProducesResponseType<CommentReactionDto[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CommentReactionDto>>> AddReactionAsync(
        [FromBody] AddReactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var reactions = await commentService.AddReactionAsync(request, cancellationToken);
        return Ok(reactions);
    }

    /// <summary>
    /// Removes a reaction from a comment.
    /// </summary>
    [HttpDelete("remove-reaction")]
    [ProducesResponseType<CommentReactionDto[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CommentReactionDto>>> RemoveReactionAsync(
        [FromQuery] long commentId,
        [FromQuery] string emojiCode,
        CancellationToken cancellationToken = default)
    {
        var reactions = await commentService.RemoveReactionAsync(commentId, emojiCode, cancellationToken);
        if (reactions == null)
        {
            throw new NotFoundException($"Reaction '{emojiCode}' for comment {commentId} was not found.");
        }

        return Ok(reactions);
    }

    /// <summary>
    /// Searches users that can be mentioned in a comment.
    /// </summary>
    [HttpGet("search-mention-users")]
    [ProducesResponseType<MentionUserDto[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<MentionUserDto>>> SearchMentionUsersAsync(
        [FromQuery] string term,
        CancellationToken cancellationToken = default)
    {
        var users = await commentService.SearchMentionUsersAsync(term, cancellationToken);
        return Ok(users);
    }
}
