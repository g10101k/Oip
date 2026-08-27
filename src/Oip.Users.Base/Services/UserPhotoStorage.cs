using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Minio.DataModel.Args;
using Oip.Users.Base.Settings;

namespace Oip.Users.Base.Services;

/// <summary>
/// Stored user photo metadata.
/// </summary>
public record StoredUserPhoto(string ObjectName, string ContentType);

/// <summary>
/// User photo content for download.
/// </summary>
public record UserPhotoContent(Stream Content, string ContentType);

/// <summary>
/// User photo storage abstraction.
/// </summary>
public interface IUserPhotoStorage
{
    /// <summary>
    /// Saves a user photo.
    /// </summary>
    Task<StoredUserPhoto> SaveAsync(int userId, IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a user photo.
    /// </summary>
    Task<UserPhotoContent> OpenReadAsync(
        string objectName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a stored user photo if present.
    /// </summary>
    Task DeleteAsync(string objectName, CancellationToken cancellationToken = default);
}

/// <summary>
/// MinIO-backed user photo storage.
/// </summary>
public class MinioUserPhotoStorage(
    UserPhotoMinioClient minioClient,
    UserPhotoStorageSettings settings,
    ILogger<MinioUserPhotoStorage> logger) : IUserPhotoStorage
{
    /// <inheritdoc />
    public async Task<StoredUserPhoto> SaveAsync(
        int userId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;
        var objectName = $"users/photos/{userId}";

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
                "Failed to save user photo {ObjectName} for user {UserId} to MinIO bucket {BucketName}.",
                objectName,
                userId,
                settings.BucketName);
            throw;
        }

        return new StoredUserPhoto(objectName, contentType);
    }

    /// <inheritdoc />
    public async Task<UserPhotoContent> OpenReadAsync(
        string objectName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();
        try
        {
            await minioClient.Client.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(settings.BucketName)
                    .WithObject(objectName)
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
                    $"MinIO returned an empty stream for user photo '{objectName}' from bucket '{settings.BucketName}'.");
            }
        }
        catch (Exception exception)
        {
            await memoryStream.DisposeAsync();
            logger.LogError(
                exception,
                "Failed to read user photo {ObjectName} from MinIO bucket {BucketName}.",
                objectName,
                settings.BucketName);
            throw;
        }

        return new UserPhotoContent(memoryStream, contentType);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        try
        {
            await minioClient.Client.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(settings.BucketName)
                    .WithObject(objectName),
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to delete user photo {ObjectName} from MinIO bucket {BucketName}.",
                objectName,
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
            await minioClient.Client.MakeBucketAsync(new MakeBucketArgs().WithBucket(settings.BucketName), cancellationToken);
        }
    }
}