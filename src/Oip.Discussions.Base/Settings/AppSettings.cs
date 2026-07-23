using Oip.Base.Settings;
using Oip.Settings;

namespace Oip.Discussions.Base.Settings;

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
        Code = "oip-discussions",
        DisplayName = "OIP.Discussions",
        BaseUrl = "https://localhost:50002",
        InternalBaseUrl = "https://localhost:5006",
        Icon = "pi pi-comments",
        Order = 60,
        Enabled = false,
        ServiceType = ServiceType.Service
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
    public AddingMode ServiceAddingMode { get; set; } = AddingMode.Service;

    /// <inheritdoc />
    public DataProtectionSettings DataProtection { get; set; } = new();

    /// <inheritdoc />
    public CorsSettings Cors { get; set; } = new();

    /// <inheritdoc />
    public ReverseProxySettings ReverseProxy { get; set; } = new();

    /// <summary>
    /// Discussion attachment storage settings.
    /// </summary>
    public DiscussionAttachmentStorageSettings DiscussionAttachmentStorage { get; set; } = new();

    /// <inheritdoc />
    public bool GenerateWebClient { get; set; }
}
