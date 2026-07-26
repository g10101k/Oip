namespace Oip.Base.Settings;

/// <summary>
/// Contains service endpoint configurations for OIP app
/// </summary>
public class OipServicesSettings
{
    /// <summary>
    /// URL endpoint for the OIP shell service
    /// </summary>
    public string Shell { get; set; } = "https://localhost:5002";

    /// <summary>
    /// URL endpoint for the OIP applications registry service
    /// </summary>
    public string ApplicationsService { get; set; } = "https://localhost:5008";

    /// <summary>
    /// URL endpoint for the OIP RTDS service
    /// </summary>
    public string RtdsService { get; set; } = "https://localhost:5003";

    /// <summary>
    /// URL endpoint for the OIP users service
    /// </summary>
    public string UsersService { get; set; } = "https://localhost:5005";

    /// <summary>
    /// URL endpoint for the OIP discussions service
    /// </summary>
    public string DiscussionsService { get; set; } = "https://localhost:5006";

    /// <summary>
    /// Gets or sets the endpoint URL for the OIP notifications service
    /// </summary>
    public string NotificationsService { get; set; } = "https://localhost:5007";
}
