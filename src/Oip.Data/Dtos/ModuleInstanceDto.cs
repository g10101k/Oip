namespace Oip.Data.Dtos;

/// <summary>
/// Module Instance Dto
/// </summary>
public class ModuleInstanceDto
{
    /// <summary>
    /// Unique identifier for the module instance.
    /// </summary>
    public int ModuleInstanceId { get; init; }

    /// <summary>
    /// Identifier for the module.
    /// </summary>
    public int ModuleId { get; init; }

    /// <summary>
    /// The label for the module instance.
    /// </summary>
    public string Label { get; init; } = null!;

    /// <summary>
    /// Icon associated with the module instance. see https://primeng.org/icons
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// Route link.
    /// </summary>
    public List<string>? RouterLink { get; init; }

    /// <summary>
    /// URL for the module instance.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// The target.
    /// </summary>
    public string? Target { get; init; }

    /// <summary>
    /// Configuration settings for the module instance.
    /// </summary>
    public string? Settings { get; init; }

    /// <summary>
    /// Child module instances.
    /// </summary>
    public List<ModuleInstanceDto>? Items { get; init; }

    /// <summary>
    /// Securities
    /// </summary>
    public List<string>? Securities { get; set; }
}