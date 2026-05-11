using Oip.Data.Entities;

namespace Oip.Data.Dtos;


/// <summary>
/// It module in app
/// </summary>
public class ModuleDto
{
    /// <summary>
    /// Id
    /// </summary>
    public int ModuleId { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Settings
    /// </summary>
    public string? Settings { get; set; }

    /// <summary>
    /// Module delivery kind.
    /// </summary>
    public ModuleKind Kind { get; set; }

    /// <summary>
    /// Extension manifest URL.
    /// </summary>
    public string? ManifestUrl { get; set; }

    /// <summary>
    /// Stable extension key.
    /// </summary>
    public string? ExtensionKey { get; set; }

    /// <summary>
    /// Custom element tag name.
    /// </summary>
    public string? ElementName { get; set; }

    /// <summary>
    /// Extension script URL.
    /// </summary>
    public string? ScriptUrl { get; set; }

    /// <summary>
    /// Extension API base URL.
    /// </summary>
    public string? ApiBaseUrl { get; set; }

    /// <summary>
    /// Extension version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Securities
    /// </summary>
    public IEnumerable<ModuleSecurityDto> ModuleSecurities { get; set; } = null!;
}
