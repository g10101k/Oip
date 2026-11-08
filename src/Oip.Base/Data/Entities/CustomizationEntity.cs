namespace Oip.Base.Data.Entities;

/// <summary>
/// Stores the site-wide assets loaded by the OIP shell.
/// </summary>
public class CustomizationEntity
{
    /// <summary>
    /// Fixed identifier of the site-wide customization record.
    /// </summary>
    public const int SiteId = 1;

    /// <summary>
    /// Primary key of the customization record.
    /// </summary>
    public int CustomizationId { get; set; }

    /// <summary>
    /// Contents of the custom stylesheet loaded by the OIP shell.
    /// </summary>
    public string Css { get; set; } = string.Empty;

    /// <summary>
    /// Contents of the custom JavaScript file loaded by the OIP shell.
    /// </summary>
    public string JavaScript { get; set; } = string.Empty;

    /// <summary>
    /// Custom favicon bytes. A null value selects the default favicon.
    /// </summary>
    public byte[]? Favicon { get; set; }
}
