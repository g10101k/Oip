namespace Oip.Discussions.Services;

/// <summary>
/// Saved attachment metadata.
/// </summary>
public record SavedDiscussionAttachment(
    Guid StorageFileId,
    string StoragePath,
    string FileName,
    string ContentType,
    long Length);

/// <summary>
/// Attachment content for download.
/// </summary>
public record DiscussionAttachmentContent(Stream Content, string ContentType, string FileName);

/// <summary>
/// Attachment storage abstraction for discussions.
/// </summary>
public interface IDiscussionAttachmentStorage
{
    /// <summary>
    /// Saves a file and returns metadata describing the stored content.
    /// </summary>
    Task<SavedDiscussionAttachment> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a stored file for reading.
    /// </summary>
    Task<DiscussionAttachmentContent> OpenReadAsync(
        string storagePath,
        string contentType,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a stored file if present.
    /// </summary>
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Local file-system storage used by discussions attachments.
/// </summary>
public class LocalDiscussionAttachmentStorage(IWebHostEnvironment environment) : IDiscussionAttachmentStorage
{
    private const string RelativeRoot = "uploads/discussions";

    /// <inheritdoc />
    public async Task<SavedDiscussionAttachment> SaveAsync(IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var storageFileId = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        var safeFileName = Path.GetFileName(file.FileName);
        var relativePath = Path.Combine(
            RelativeRoot,
            $"{DateTime.UtcNow:yyyy/MM/dd}",
            $"{storageFileId}{extension}");
        var absolutePath = GetAbsolutePath(relativePath);

        var directoryPath = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await using var target = File.Create(absolutePath);
        await using var source = file.OpenReadStream();
        await source.CopyToAsync(target, cancellationToken);

        return new SavedDiscussionAttachment(
            storageFileId,
            relativePath.Replace('\\', '/'),
            safeFileName,
            file.ContentType,
            file.Length);
    }

    /// <inheritdoc />
    public Task<DiscussionAttachmentContent> OpenReadAsync(
        string storagePath,
        string contentType,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(storagePath);
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("Attachment content was not found.", absolutePath);
        }

        Stream stream = File.OpenRead(absolutePath);
        return Task.FromResult(new DiscussionAttachmentContent(stream, contentType, fileName));
    }

    /// <inheritdoc />
    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(storagePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    private string GetAbsolutePath(string relativePath)
    {
        var rootPath = environment.ContentRootPath;
        return Path.Combine(rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}