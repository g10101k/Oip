using Minio;

namespace Oip.Discussions.Base.Services;

/// <summary>
/// Dedicated MinIO client wrapper for discussion attachments.
/// </summary>
public class DiscussionAttachmentMinioClient(IMinioClient client)
{
    /// <summary>
    /// Underlying MinIO client instance.
    /// </summary>
    public IMinioClient Client { get; } = client;
}
