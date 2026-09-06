using Oip.Base.Security.DefaultSecrets;
using Oip.Base.Settings.Attributes;

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
    public string AccessKey { get; set; } = KnownDefaultSecrets.ObjectStorageAccessKey;

    /// <summary>
    /// Secret key.
    /// </summary>
    [SecretSetting(KnownDefaultSecrets.ObjectStorageSecretKey)]
    public string SecretKey { get; set; } = KnownDefaultSecrets.ObjectStorageSecretKey;

    /// <summary>
    /// Bucket name for discussion attachments.
    /// </summary>
    public string BucketName { get; set; } = "oip";

    /// <summary>
    /// Whether HTTPS should be used for object storage requests.
    /// </summary>
    public bool UseSsl { get; set; }
}
