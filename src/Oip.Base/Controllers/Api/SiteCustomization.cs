using System.ComponentModel.DataAnnotations;

namespace Oip.Base.Controllers.Api;

/// <summary>
/// Site-wide CSS, JavaScript, and an optional favicon.
/// </summary>
public class SiteCustomization
{
    internal const int MaxFaviconBytes = 256 * 1024;

    /// <summary>
    /// Contents of the custom stylesheet. An empty string clears the stylesheet.
    /// </summary>
    [Required(AllowEmptyStrings = true), MaxLength(512 * 1024)]
    public required string Css { get; init; }

    /// <summary>
    /// Contents of the custom script. An empty string clears the script.
    /// </summary>
    [Required(AllowEmptyStrings = true), MaxLength(512 * 1024)]
    public required string JavaScript { get; init; }

    /// <summary>
    /// Base64-encoded PNG or ICO image. A null value selects the default favicon.
    /// </summary>
    [MaxLength((MaxFaviconBytes + 2) / 3 * 4)]
    public string? FaviconBase64 { get; init; }
}
