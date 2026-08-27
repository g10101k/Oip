using Minio;

namespace Oip.Users.Base.Services;

/// <summary>
/// Dedicated MinIO client wrapper for user photos.
/// </summary>
public class UserPhotoMinioClient(IMinioClient client)
{
    /// <summary>
    /// Underlying MinIO client instance.
    /// </summary>
    public IMinioClient Client { get; } = client;
}
