namespace Oip.Base.Data.Entities;

/// <summary>
/// Module instance
/// </summary>
public class ModuleInstanceEntity
{
    /// <summary>
    /// Id
    /// </summary>
    public int ModuleInstanceId { get; set; }

    /// <summary>
    /// Module id
    /// </summary>
    public int ModuleId { get; set; }

    /// <summary>
    /// Label
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// Label
    /// </summary>
    public string? Icon { get; set; } = null!;

    /// <summary>
    /// Url
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Target
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Parent id
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Parent instance
    /// </summary>
    public ModuleInstanceEntity? Parent { get; set; }

    /// <summary>
    /// Children
    /// </summary>
    public List<ModuleInstanceEntity> Items { get; set; } = new();

    /// <summary>
    /// Securities
    /// </summary>
    public List<ModuleInstanceSecurityEntity> Securities { get; set; } = new();

    /// <summary>
    /// Settings
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Settings { get; set; } = null!;

    /// <summary>
    /// Module
    /// </summary>
    public ModuleEntity Module { get; set; } = null!;
}