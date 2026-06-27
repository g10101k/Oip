namespace Oip.Users.Base.Settings;

/// <summary>
/// User photo object storage settings.
/// </summary>
public class UserPhotoStorageSettings
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
    /// Bucket name for user photos.
    /// </summary>
    public string BucketName { get; set; } = "oip-user-photos";

    /// <summary>
    /// Whether HTTPS should be used for object storage requests.
    /// </summary>
    public bool UseSsl { get; set; }
}
