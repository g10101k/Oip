namespace Oip.Base.Security.DefaultSecrets;

/// <summary>
/// Single registry of the secret values shipped with the repository. The values are already public because they are
/// committed to the repository, and they are kept as literals so that the startup validator can name the offending
/// setting instead of reporting an opaque hash.
/// </summary>
public static class KnownDefaultSecrets
{
    /// <summary>
    /// Keycloak client secret shipped in every service <c>appsettings.json</c>.
    /// </summary>
    public const string KeycloakClientSecret = "Xnw6h8wH0dgehxGxZYgWKbKosKCx9E8L";

    /// <summary>
    /// Keycloak admin password used by the development and test containers.
    /// </summary>
    public const string KeycloakAdminPassword = "P@ssw0rd";

    /// <summary>
    /// MinIO/S3 access key shipped as a code default. It is the root user of the development container.
    /// </summary>
    public const string ObjectStorageAccessKey = "admin";

    /// <summary>
    /// MinIO/S3 secret key shipped as a code default and in <c>appsettings.json</c>.
    /// </summary>
    public const string ObjectStorageSecretKey = "P@ssw0rd";

    /// <summary>
    /// Redis connection string of the development container, carrying the sample password.
    /// </summary>
    public const string AuthTicketStoreRedisConnectionString = "localhost:6379,password=P@ssw0rd,defaultDatabase=0";

    /// <summary>
    /// Shared secret used to validate Keycloak event callbacks.
    /// </summary>
    public const string KeycloakEventsSharedSecret = "change-me-keycloak-events";

    /// <summary>
    /// SMTP password shipped as a data protection blob that cannot be unprotected outside the sample environment.
    /// </summary>
    public const string SmtpPassword =
        "CfDJ8HC2fbULANdNhgPoKimUG23QvgSAgULIOlvuJngp5Cxk8DqYx9PYNGrhk_2DOd9XrUsCcZv2xzJglieYPKIusSu_3icVX0axyKg0ZDBb33x_r20SKULloOGpbBQXpwjmSA";

    /// <summary>
    /// Certificate password used by the development Kestrel endpoints.
    /// </summary>
    public const string DevelopmentCertificatePassword = "P@ssw0rd";

    /// <summary>
    /// Secret bearing configuration keys that are not part of the <see cref="Settings.ISettings"/> object graph
    /// and therefore cannot be discovered through <see cref="Settings.Attributes.SecretSettingAttribute"/>.
    /// </summary>
    public static IReadOnlyList<RawSecretSetting> RawSecrets { get; } =
    [
        new("SmtpSettings:SmtpPassword", [SmtpPassword], Required: false),
        new("Kestrel:Endpoints:Https:Certificate:Password", [DevelopmentCertificatePassword], Required: false)
    ];
}

/// <summary>
/// A secret bearing configuration key declared by key instead of by attribute.
/// </summary>
/// <param name="ConfigKey">Configuration key of the setting.</param>
/// <param name="InsecureValues">Values shipped with the repository.</param>
/// <param name="Required">Whether an empty value has to be reported as a finding.</param>
/// <param name="OverrideHint">Hint explaining how to supply a real value.</param>
public sealed record RawSecretSetting(
    string ConfigKey,
    IReadOnlyList<string> InsecureValues,
    bool Required = true,
    string? OverrideHint = null);
