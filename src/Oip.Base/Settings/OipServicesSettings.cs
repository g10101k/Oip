namespace Oip.Base.Settings;

/// <summary>
/// Contains service endpoint configurations for OIP app
/// </summary>
public class OipServicesSettings
{
    /// <summary>
    /// URL endpoint for the OIP shell service
    /// </summary>
    public string Oip { get; set; } = "https://localhost:5002";

    /// <summary>
    /// URL endpoint for the OIP users service
    /// </summary>
    public string OipUsers { get; set; } = "https://localhost:5005";

    /// <summary>
    /// URL endpoint for the OIP discussions service
    /// </summary>
    public string OipDiscussions { get; set; } = "https://localhost:5006";

    /// <summary>
    /// Gets or sets the endpoint URL for the OIP notifications service
    /// </summary>
    public string OipNotifications { get; set; } = "https://localhost:5007";
}
