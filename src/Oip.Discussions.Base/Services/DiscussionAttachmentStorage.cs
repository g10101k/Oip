using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Minio.DataModel.Args;
using Oip.Discussions.Base.Settings;

namespace Oip.Discussions.Base.Services;

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
    Task<SavedDiscussionAttachment> SaveAsync(
        long objectTypeId,
        long objectId,
        IFormFile file,
        CancellationToken cancellationToken = default);

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
/// MinIO-backed storage for discussion attachments.
/// </summary>
public class MinioDiscussionAttachmentStorage(
    DiscussionAttachmentMinioClient minioClient,
    DiscussionAttachmentStorageSettings settings,
    ILogger<MinioDiscussionAttachmentStorage> logger) : IDiscussionAttachmentStorage
{
    /// <inheritdoc />
    public async Task<SavedDiscussionAttachment> SaveAsync(
        long objectTypeId,
        long objectId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var storageFileId = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        var safeFileName = Path.GetFileName(file.FileName);
        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;
        var objectName = $"discussions/{objectTypeId}/{objectId}/attachments/{storageFileId}{extension}";

        try
        {
            await EnsureBucketExistsAsync(cancellationToken);

            await using var stream = file.OpenReadStream();
            await minioClient.Client.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(settings.BucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(file.Length)
                    .WithContentType(contentType),
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to save discussion attachment {ObjectName} to MinIO bucket {BucketName}.",
                objectName,
                settings.BucketName);
            throw;
        }

        return new SavedDiscussionAttachment(storageFileId, objectName, safeFileName, contentType, file.Length);
    }

    /// <inheritdoc />
    public async Task<DiscussionAttachmentContent> OpenReadAsync(
        string storagePath,
        string contentType,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();
        try
        {
            await minioClient.Client.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(settings.BucketName)
                    .WithObject(storagePath)
                    .WithCallbackStream(stream =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                    }),
                cancellationToken);

            if (memoryStream.Length == 0)
            {
                throw new InvalidOperationException(
                    $"MinIO returned an empty stream for discussion attachment '{storagePath}' from bucket '{settings.BucketName}'.");
            }
        }
        catch (Exception exception)
        {
            await memoryStream.DisposeAsync();
            logger.LogError(
                exception,
                "Failed to read discussion attachment {ObjectName} from MinIO bucket {BucketName}.",
                storagePath,
                settings.BucketName);
            throw;
        }

        return new DiscussionAttachmentContent(memoryStream, contentType, fileName);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await minioClient.Client.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(settings.BucketName)
                    .WithObject(storagePath),
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to delete discussion attachment {ObjectName} from MinIO bucket {BucketName}.",
                storagePath,
                settings.BucketName);
            throw;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var exists = await minioClient.Client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(settings.BucketName),
            cancellationToken);
        if (!exists)
        {
            await minioClient.Client.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(settings.BucketName),
                cancellationToken);
        }
    }
}
