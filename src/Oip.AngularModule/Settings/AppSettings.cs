using Oip.Base.Settings;
using Oip.Settings;
using Oip.Users.Base.Settings;

namespace Oip.AngularModule.Settings;

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
        Code = "oip-angular-module",
        DisplayName = "OIP Angular Module",
        BaseUrl = "https://localhost:50008",
        InternalBaseUrl = "https://localhost:5008",
        Icon = "pi pi-th-large",
        Order = 100,
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
    public CorsSettings Cors { get; set; } = new();

    /// <inheritdoc />
    public ReverseProxySettings ReverseProxy { get; set; } = new();

    /// <inheritdoc />
    public bool GenerateWebClient { get; set; } = false;

    /// <inheritdoc />
    public AddingMode ServiceAddingMode { get; set; } = AddingMode.Local;

    /// <inheritdoc />
    public DataProtectionSettings DataProtection { get; set; } = new();
    
    public KeycloakSyncSettings KeycloakSyncSettings { get; set; } = new();

    /// <summary>
    /// User photo storage
    /// </summary>
    public UserPhotoStorageSettings UserPhotoStorage { get; set; } = new();
}
