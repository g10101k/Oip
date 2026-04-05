using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Exceptions;
using Oip.Discussions.Contracts;
using Oip.Discussions.Data;
using Oip.Users.Entities;
using Oip.Users.Repositories;
using BaseUserService = Oip.Base.Services.UserService;

namespace Oip.Discussions.Services;

/// <summary>
/// Business logic for entity comments.
/// </summary>
public class CommentService(
    DiscussionsDbContext discussionsDbContext,
    UserRepository userRepository,
    BaseUserService currentUserService,
    IDiscussionAttachmentStorage attachmentStorage)
{
    private const int MaxPageSize = 100;
    private const int MaxFileSizeBytes = 10 * 1024 * 1024;
    private static readonly Regex MentionRegex = new(@"@(?<token>[A-Za-z0-9._+\-@]{2,100})", RegexOptions.Compiled);
    private static readonly HashSet<string> AllowedFileTypes =
    [
        "image/png",
        "image/jpeg",
        "image/gif",
        "image/webp",
        "application/pdf",
        "text/plain",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    ];

    /// <summary>
    /// Gets comments for a target entity.
    /// </summary>
    public async Task<IReadOnlyList<CommentDto>> GetByObjectAsync(
        long objectTypeId,
        long objectId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        skip = Math.Max(skip, 0);
        take = NormalizeTake(take);

        var currentUserId = await GetCurrentUserIdIfAvailableAsync(cancellationToken);
        var comments = await discussionsDbContext.Comments
            .AsNoTracking()
            .Where(c => c.ObjectTypeId == objectTypeId && c.ObjectId == objectId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return await MapCommentsAsync(comments, currentUserId, cancellationToken);
    }

    /// <summary>
    /// Gets a single comment by identifier.
    /// </summary>
    public async Task<CommentDto?> GetByIdAsync(long commentId, CancellationToken cancellationToken = default)
    {
        var currentUserId = await GetCurrentUserIdIfAvailableAsync(cancellationToken);
        var comment = await discussionsDbContext.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CommentId == commentId, cancellationToken);

        if (comment == null)
        {
            return null;
        }

        var comments = await MapCommentsAsync([comment], currentUserId, cancellationToken);
        return comments.Single();
    }

    /// <summary>
    /// Creates a new comment.
    /// </summary>
    public async Task<CommentDto> CreateAsync(
        CreateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var content = NormalizeContent(request.Content);

        var comment = new CommentEntity
        {
            ObjectTypeId = request.ObjectTypeId,
            ObjectId = request.ObjectId,
            Content = content,
            UserId = currentUser.UserId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        discussionsDbContext.Comments.Add(comment);
        await discussionsDbContext.SaveChangesAsync(cancellationToken);

        await ReplaceMentionsAsync(comment.CommentId, content, cancellationToken);
        return await GetRequiredCommentAsync(comment.CommentId, cancellationToken);
    }

    /// <summary>
    /// Updates an existing comment.
    /// </summary>
    public async Task<CommentDto?> UpdateAsync(
        long commentId,
        UpdateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var comment = await discussionsDbContext.Comments
            .FirstOrDefaultAsync(c => c.CommentId == commentId, cancellationToken);

        if (comment == null)
        {
            return null;
        }

        EnsureCommentOwnership(comment, currentUser.UserId);

        var newContent = NormalizeContent(request.Content);
        if (string.Equals(comment.Content, newContent, StringComparison.Ordinal))
        {
            throw new CommentValidationException("Comment content was not changed.");
        }

        discussionsDbContext.CommentEditHistory.Add(new CommentEditHistoryEntity
        {
            CommentId = comment.CommentId,
            OldContent = comment.Content,
            NewContent = newContent,
            EditedByUserId = currentUser.UserId,
            EditedAt = DateTimeOffset.UtcNow
        });

        comment.Content = newContent;
        comment.UpdatedAt = DateTimeOffset.UtcNow;
        await discussionsDbContext.SaveChangesAsync(cancellationToken);

        await ReplaceMentionsAsync(comment.CommentId, newContent, cancellationToken);
        return await GetRequiredCommentAsync(commentId, cancellationToken);
    }

    /// <summary>
    /// Soft deletes a comment.
    /// </summary>
    public async Task<bool> DeleteAsync(long commentId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var comment = await discussionsDbContext.Comments
            .Include(c => c.Attachments)
            .FirstOrDefaultAsync(c => c.CommentId == commentId, cancellationToken);

        if (comment == null)
        {
            return false;
        }

        EnsureCommentOwnership(comment, currentUser.UserId);

        foreach (var attachment in comment.Attachments)
        {
            await attachmentStorage.DeleteAsync(attachment.FilePath, cancellationToken);
        }

        comment.IsDeleted = true;
        comment.DeletedAt = DateTimeOffset.UtcNow;
        comment.UpdatedAt = DateTimeOffset.UtcNow;

        await discussionsDbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Gets the edit history for a comment.
    /// </summary>
    public async Task<IReadOnlyList<CommentHistoryDto>?> GetHistoryAsync(
        long commentId,
        CancellationToken cancellationToken = default)
    {
        var commentExists = await discussionsDbContext.Comments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(c => c.CommentId == commentId, cancellationToken);

        if (!commentExists)
        {
            return null;
        }

        var history = await discussionsDbContext.CommentEditHistory
            .AsNoTracking()
            .Where(h => h.CommentId == commentId)
            .OrderByDescending(h => h.EditedAt)
            .ToListAsync(cancellationToken);

        if (history.Count == 0)
        {
            return [];
        }

        var editors = await GetUsersByIdsAsync(history.Select(x => x.EditedByUserId), cancellationToken);
        return history.Select(entry =>
        {
            editors.TryGetValue((int)entry.EditedByUserId, out var editor);
            return new CommentHistoryDto
            {
                CommentEditHistoryId = entry.CommentEditHistoryId,
                OldContent = entry.OldContent,
                NewContent = entry.NewContent,
                EditedByUserId = entry.EditedByUserId,
                EditedByDisplayName = FormatUserDisplayName(editor, entry.EditedByUserId),
                EditedAt = entry.EditedAt
            };
        }).ToList();
    }

    /// <summary>
    /// Uploads an attachment to an existing comment.
    /// </summary>
    public async Task<AttachmentDto> UploadAttachmentAsync(
        UploadAttachmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var comment = await discussionsDbContext.Comments
            .FirstOrDefaultAsync(c => c.CommentId == request.CommentId, cancellationToken);

        if (comment == null)
        {
            throw new NotFoundException($"Comment {request.CommentId} was not found.");
        }

        EnsureCommentOwnership(comment, currentUser.UserId);
        ValidateAttachment(request.File);

        var storageFile = await attachmentStorage.SaveAsync(request.File, cancellationToken);
        var attachment = new AttachmentEntity
        {
            CommentId = comment.CommentId,
            FileName = storageFile.FileName,
            FilePath = storageFile.StoragePath,
            FileType = storageFile.ContentType,
            FileSize = storageFile.Length,
            UploadedAt = DateTimeOffset.UtcNow,
            StorageFileId = storageFile.StorageFileId
        };

        discussionsDbContext.Attachments.Add(attachment);
        await discussionsDbContext.SaveChangesAsync(cancellationToken);

        return MapAttachment(attachment);
    }

    /// <summary>
    /// Deletes an attachment.
    /// </summary>
    public async Task<bool> DeleteAttachmentAsync(long attachmentId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var attachment = await discussionsDbContext.Attachments
            .Include(x => x.Comment)
            .FirstOrDefaultAsync(x => x.AttachmentId == attachmentId, cancellationToken);

        if (attachment == null)
        {
            return false;
        }

        EnsureCommentOwnership(attachment.Comment, currentUser.UserId);

        await attachmentStorage.DeleteAsync(attachment.FilePath, cancellationToken);
        discussionsDbContext.Attachments.Remove(attachment);
        await discussionsDbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Opens an attachment stream for download.
    /// </summary>
    public async Task<DiscussionAttachmentContent?> GetAttachmentContentAsync(
        long attachmentId,
        CancellationToken cancellationToken = default)
    {
        var attachment = await discussionsDbContext.Attachments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AttachmentId == attachmentId, cancellationToken);

        if (attachment == null)
        {
            return null;
        }

        return await attachmentStorage.OpenReadAsync(
            attachment.FilePath,
            attachment.FileType,
            attachment.FileName,
            cancellationToken);
    }

    /// <summary>
    /// Toggles a reaction for the current user.
    /// </summary>
    public async Task<IReadOnlyList<CommentReactionDto>> AddReactionAsync(
        AddReactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var comment = await discussionsDbContext.Comments
            .FirstOrDefaultAsync(c => c.CommentId == request.CommentId, cancellationToken);

        if (comment == null)
        {
            throw new NotFoundException($"Comment {request.CommentId} was not found.");
        }

        var emojiCode = NormalizeEmojiCode(request.EmojiCode);
        var existingReaction = await discussionsDbContext.Reactions
            .FirstOrDefaultAsync(
                x => x.CommentId == request.CommentId && x.UserId == currentUser.UserId,
                cancellationToken);

        if (existingReaction != null)
        {
            if (string.Equals(existingReaction.EmojiCode, emojiCode, StringComparison.Ordinal))
            {
                discussionsDbContext.Reactions.Remove(existingReaction);
            }
            else
            {
                existingReaction.EmojiCode = emojiCode;
                existingReaction.ReactedAt = DateTimeOffset.UtcNow;
            }
        }
        else
        {
            discussionsDbContext.Reactions.Add(new ReactionEntity
            {
                CommentId = request.CommentId,
                UserId = currentUser.UserId,
                EmojiCode = emojiCode,
                ReactedAt = DateTimeOffset.UtcNow
            });
        }

        await discussionsDbContext.SaveChangesAsync(cancellationToken);
        var dictionary = await BuildReactionDtosAsync([request.CommentId], currentUser.UserId, cancellationToken);
        return dictionary.TryGetValue(request.CommentId, out var value) ? value : [];
    }

    /// <summary>
    /// Removes a reaction from a comment.
    /// </summary>
    public async Task<IReadOnlyList<CommentReactionDto>?> RemoveReactionAsync(
        long commentId,
        string emojiCode,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var reaction = await discussionsDbContext.Reactions
            .FirstOrDefaultAsync(
                x => x.CommentId == commentId && x.UserId == currentUser.UserId && x.EmojiCode == NormalizeEmojiCode(emojiCode),
                cancellationToken);

        if (reaction == null)
        {
            return null;
        }

        discussionsDbContext.Reactions.Remove(reaction);
        await discussionsDbContext.SaveChangesAsync(cancellationToken);

        var dictionary = await BuildReactionDtosAsync([commentId], currentUser.UserId, cancellationToken);
        return dictionary.TryGetValue(commentId, out var items) ? items : [];
    }

    /// <summary>
    /// Searches users that can be mentioned.
    /// </summary>
    public async Task<IReadOnlyList<MentionUserDto>> SearchMentionUsersAsync(
        string term,
        CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term.Trim();
        if (normalizedTerm.Length < 2)
        {
            throw new CommentValidationException("Search term must contain at least 2 characters.");
        }

        var users = await userRepository.SearchAsync(normalizedTerm);
        return users
            .Take(20)
            .Select(user => new MentionUserDto
            {
                UserId = user.UserId,
                DisplayName = FormatUserDisplayName(user, user.UserId),
                Email = user.Email
            })
            .ToList();
    }

    private async Task<CommentDto> GetRequiredCommentAsync(long commentId, CancellationToken cancellationToken)
    {
        var comment = await GetByIdAsync(commentId, cancellationToken);
        return comment ?? throw new NotFoundException($"Comment {commentId} was not found.");
    }

    private async Task<IReadOnlyList<CommentDto>> MapCommentsAsync(
        IReadOnlyCollection<CommentEntity> comments,
        long currentUserId,
        CancellationToken cancellationToken)
    {
        if (comments.Count == 0)
        {
            return [];
        }

        var commentIds = comments.Select(c => c.CommentId).ToList();
        var userLookup = await GetUsersByIdsAsync(comments.Select(c => c.UserId), cancellationToken);
        var historyCounts = await discussionsDbContext.CommentEditHistory
            .AsNoTracking()
            .Where(h => commentIds.Contains(h.CommentId))
            .GroupBy(h => h.CommentId)
            .Select(g => new { CommentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CommentId, x => x.Count, cancellationToken);

        var attachments = await discussionsDbContext.Attachments
            .AsNoTracking()
            .Where(x => commentIds.Contains(x.CommentId))
            .OrderBy(x => x.UploadedAt)
            .ToListAsync(cancellationToken);

        var mentions = await discussionsDbContext.Mentions
            .AsNoTracking()
            .Where(x => commentIds.Contains(x.CommentId))
            .OrderBy(x => x.Position)
            .ToListAsync(cancellationToken);

        var mentionUsers = await GetUsersByIdsAsync(mentions.Select(x => x.MentionedUserId), cancellationToken);
        var reactions = await BuildReactionDtosAsync(commentIds, currentUserId, cancellationToken);

        return comments.Select(comment =>
        {
            historyCounts.TryGetValue(comment.CommentId, out var historyCount);
            userLookup.TryGetValue((int)comment.UserId, out var user);

            return new CommentDto
            {
                CommentId = comment.CommentId,
                ObjectTypeId = comment.ObjectTypeId,
                ObjectId = comment.ObjectId,
                Content = comment.Content,
                UserId = comment.UserId,
                AuthorDisplayName = FormatUserDisplayName(user, comment.UserId),
                AuthorEmail = user?.Email ?? string.Empty,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                IsEdited = comment.UpdatedAt.HasValue,
                HistoryCount = historyCount,
                CanEdit = comment.UserId == currentUserId,
                CanDelete = comment.UserId == currentUserId,
                Attachments = attachments.Where(x => x.CommentId == comment.CommentId).Select(MapAttachment).ToList(),
                Mentions = mentions
                    .Where(x => x.CommentId == comment.CommentId)
                    .Select(x =>
                    {
                        mentionUsers.TryGetValue((int)x.MentionedUserId, out var mentionedUser);
                        return new CommentMentionDto
                        {
                            MentionedUserId = x.MentionedUserId,
                            DisplayName = FormatUserDisplayName(mentionedUser, x.MentionedUserId),
                            Email = mentionedUser?.Email ?? string.Empty,
                            Position = x.Position
                        };
                    })
                    .ToList(),
                Reactions = reactions.TryGetValue(comment.CommentId, out var commentReactions) ? commentReactions : []
            };
        }).ToList();
    }

    private async Task<Dictionary<long, List<CommentReactionDto>>> BuildReactionDtosAsync(
        IReadOnlyCollection<long> commentIds,
        long currentUserId,
        CancellationToken cancellationToken)
    {
        if (commentIds.Count == 0)
        {
            return [];
        }

        var grouped = await discussionsDbContext.Reactions
            .AsNoTracking()
            .Where(x => commentIds.Contains(x.CommentId))
            .GroupBy(x => new { x.CommentId, x.EmojiCode })
            .Select(g => new
            {
                g.Key.CommentId,
                g.Key.EmojiCode,
                Count = g.Count(),
                ReactedByCurrentUser = currentUserId > 0 && g.Any(x => x.UserId == currentUserId)
            })
            .OrderBy(x => x.EmojiCode)
            .ToListAsync(cancellationToken);

        return grouped
            .GroupBy(x => x.CommentId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(item => new CommentReactionDto
                {
                    EmojiCode = item.EmojiCode,
                    Count = item.Count,
                    ReactedByCurrentUser = item.ReactedByCurrentUser
                }).ToList());
    }

    private async Task ReplaceMentionsAsync(long commentId, string content, CancellationToken cancellationToken)
    {
        var tokens = MentionRegex.Matches(content)
            .Select(match => new MentionToken(match.Groups["token"].Value, match.Index))
            .DistinctBy(x => (x.Value, x.Position))
            .ToList();

        var mentions = new List<MentionEntity>();
        if (tokens.Count > 0)
        {
            var candidates = await ResolveMentionUsersAsync(tokens.Select(x => x.Value), cancellationToken);
            mentions.AddRange(tokens
                .Where(token => candidates.TryGetValue(token.Value, out _))
                .Select(token =>
                {
                    var user = candidates[token.Value];
                    return new MentionEntity
                    {
                        CommentId = commentId,
                        MentionedUserId = user.UserId,
                        Position = token.Position,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                }));
        }

        var existing = await discussionsDbContext.Mentions
            .Where(x => x.CommentId == commentId)
            .ToListAsync(cancellationToken);

        discussionsDbContext.Mentions.RemoveRange(existing);
        if (mentions.Count > 0)
        {
            discussionsDbContext.Mentions.AddRange(mentions);
        }

        await discussionsDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<string, UserEntity>> ResolveMentionUsersAsync(
        IEnumerable<string> tokens,
        CancellationToken cancellationToken)
    {
        var tokenList = tokens
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (tokenList.Count == 0)
        {
            return [];
        }

        var lowered = tokenList.Select(x => x.ToLowerInvariant()).ToList();
        var users = await userRepository.GetAllAsync(0, 500);

        return users
            .Where(user => MatchesMentionCandidate(user, lowered))
            .SelectMany(user =>
            {
                var keys = new[]
                {
                    user.Email,
                    user.KeycloakId,
                    $"{user.FirstName}.{user.LastName}",
                    $"{user.FirstName}{user.LastName}",
                    $"{user.FirstName} {user.LastName}"
                };

                return keys
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => new { Key = x!.ToLowerInvariant(), User = user });
            })
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.First().User);
    }

    private static bool MatchesMentionCandidate(UserEntity user, IReadOnlyCollection<string> loweredTokens)
    {
        var candidates = new[]
        {
            user.Email,
            user.KeycloakId,
            $"{user.FirstName}.{user.LastName}",
            $"{user.FirstName}{user.LastName}",
            $"{user.FirstName} {user.LastName}"
        };

        return candidates
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.ToLowerInvariant())
            .Any(loweredTokens.Contains);
    }

    private static AttachmentDto MapAttachment(AttachmentEntity attachment)
    {
        return new AttachmentDto
        {
            AttachmentId = attachment.AttachmentId,
            FileName = attachment.FileName,
            FileType = attachment.FileType,
            FileSize = attachment.FileSize,
            UploadedAt = attachment.UploadedAt,
            StorageFileId = attachment.StorageFileId,
            DownloadUrl = $"/api/comments/get-attachment-content/{attachment.AttachmentId}"
        };
    }

    private async Task<UserEntity> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var email = GetCurrentUserEmail();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UnauthorizedDiscussionException("Current user e-mail claim was not found.");
        }

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            throw new CommentValidationException($"User profile for '{email}' was not found.");
        }

        return user;
    }

    private async Task<long> GetCurrentUserIdIfAvailableAsync(CancellationToken cancellationToken)
    {
        var email = GetCurrentUserEmail();
        if (string.IsNullOrWhiteSpace(email))
        {
            return 0;
        }

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        return user?.UserId ?? 0;
    }

    private async Task<Dictionary<int, UserEntity>> GetUsersByIdsAsync(
        IEnumerable<long> userIds,
        CancellationToken cancellationToken)
    {
        var ids = userIds
            .Select(x => (int)x)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        var users = await userRepository.GetByIdsAsync(ids, cancellationToken);
        return users.ToDictionary(x => x.UserId);
    }

    private static int NormalizeTake(int take)
    {
        if (take <= 0)
        {
            return 50;
        }

        return Math.Min(take, MaxPageSize);
    }

    private static string NormalizeContent(string content)
    {
        var normalizedContent = content.Trim();
        if (string.IsNullOrWhiteSpace(normalizedContent))
        {
            throw new CommentValidationException("Comment content is required.");
        }

        if (normalizedContent.Length > 4000)
        {
            throw new CommentValidationException("Comment content exceeds the 4000 character limit.");
        }

        return normalizedContent;
    }

    private static string NormalizeEmojiCode(string emojiCode)
    {
        var normalized = emojiCode.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new CommentValidationException("Emoji code is required.");
        }

        if (normalized.Length > 20)
        {
            throw new CommentValidationException("Emoji code exceeds the 20 character limit.");
        }

        return normalized;
    }

    private static void ValidateAttachment(IFormFile file)
    {
        if (file.Length <= 0)
        {
            throw new CommentValidationException("Attachment file is empty.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new CommentValidationException("Attachment exceeds the 10 MB limit.");
        }

        if (!AllowedFileTypes.Contains(file.ContentType))
        {
            throw new CommentValidationException($"Attachment type '{file.ContentType}' is not supported.");
        }
    }

    private static void EnsureCommentOwnership(CommentEntity comment, long currentUserId)
    {
        if (comment.UserId != currentUserId)
        {
            throw new CommentAccessException("Only the author can modify this comment.");
        }
    }

    private static string FormatUserDisplayName(UserEntity? user, long fallbackUserId)
    {
        if (user == null)
        {
            return $"User #{fallbackUserId}";
        }

        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.Email : fullName;
    }

    private string? GetCurrentUserEmail()
    {
        return currentUserService.GetUserEmail();
    }

    private sealed record MentionToken(string Value, int Position);
}

/// <summary>
/// Validation exception for comment operations.
/// </summary>
public class CommentValidationException(string message)
    : ApiException("Validation error", message, StatusCodes.Status400BadRequest);

/// <summary>
/// Access exception for comment operations.
/// </summary>
public class CommentAccessException(string message)
    : ApiException("Access denied", message, StatusCodes.Status403Forbidden);

/// <summary>
/// Not found exception for comment resources.
/// </summary>
public class NotFoundException(string message)
    : ApiException("Not found", message, StatusCodes.Status404NotFound);

/// <summary>
/// Unauthorized exception for discussion operations.
/// </summary>
public class UnauthorizedDiscussionException(string message)
    : ApiException("Unauthorized", message, StatusCodes.Status401Unauthorized);
