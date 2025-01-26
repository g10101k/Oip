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
    /// Securities
    /// </summary>
    public IEnumerable<ModuleSecurityDto> ModuleSecurities { get; set; } = null!;
}