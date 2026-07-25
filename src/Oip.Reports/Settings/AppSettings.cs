using Oip.Base.Settings;
using Oip.Discussions.Base.Settings;
using Oip.Settings;
using Oip.Users.Base.Settings;

namespace Oip.Reports.Settings;

/// <summary>
/// Application settings
/// </summary>
public class AppSettings : BaseAppSettings<AppSettings>, ISettings
{
    /// <inheritdoc />
    public OipServicesSettings Services { get; set; } = new();

    /// <inheritdoc />
    public ApplicationSettings Application { get; set; } = new()
    {
        Code = "oip-reports",
        DisplayName = "OIP  Reports",
        BaseUrl = "https://localhost:50009",
        InternalBaseUrl = "https://localhost:5009",
        Icon = "pi pi-home",
        Order = 10,
        ServiceType = ServiceType.Application
    };

    /// <inheritdoc />
    public ApplicationRegistrySettings ApplicationRegistry { get; set; } = new();

    /// <inheritdoc />
    public OpenApiSettings OpenApi { get; set; } = new();

    /// <inheritdoc />
    public SpaDevelopmentServerSettings SpaProxyServer { get; set; } = new();

    /// <inheritdoc />
    public SecurityServiceSettings SecurityService { get; set; } = new();

    /// <inheritdoc />
    public OpenTelemetrySettings OpenTelemetry { get; set; } = new();

    /// <inheritdoc />
    public AddingMode ServiceAddingMode { get; set; } = AddingMode.Local;

    /// <inheritdoc />
    public DataProtectionSettings DataProtection { get; set; } = new();

    /// <inheritdoc />
    public ReverseProxySettings ReverseProxy { get; set; } = new();

    /// <inheritdoc />
    public bool GenerateWebClient { get; set; }

    /// <summary>
    /// CORS Settings
    /// </summary>
    public CorsSettings Cors { get; set; } = new();

    /// <summary>
    /// Keycloak synchronization settings
    /// </summary>
    public KeycloakSyncSettings KeycloakSync { get; set; } = new();

    /// <summary>
    /// User photo storage
    /// </summary>
    public UserPhotoStorageSettings UserPhotoStorage { get; set; } = new();

    /// <summary>
    /// Discussion attachment storage settings.
    /// </summary>
    public DiscussionAttachmentStorageSettings DiscussionAttachmentStorage { get; set; } = new();
}
