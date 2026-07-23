namespace Oip.Base.Settings;

/// <summary>
/// Object storage settings.
/// </summary>
public class ObjectStorageSettings
{
    /// <summary>
    /// MinIO/S3 endpoint without protocol, for example localhost:9000.
    /// </summary>
    public string Endpoint { get; set; } = "localhost:9000";

    /// <summary>
    /// Access key.
    /// </summary>
    public string AccessKey { get; set; } = "admin";

    /// <summary>
    /// Secret key.
    /// </summary>
    public string SecretKey { get; set; } = "P@ssw0rd";

    /// <summary>
    /// Bucket name for discussion attachments.
    /// </summary>
    public string BucketName { get; set; } = "oip";

    /// <summary>
    /// Whether HTTPS should be used for object storage requests.
    /// </summary>
    public bool UseSsl { get; set; }
}
