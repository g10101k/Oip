namespace Oip.Data.Entities;

/// <summary>
/// Module entity
/// </summary>
public class ModuleEntity
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
    /// Settings for module
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? Settings { get; set; }
    
    /// <summary>
    /// Route link to component
    /// </summary>
    public string? RouterLink { get; set; }
    
    /// <summary>
    /// Module Securities
    /// </summary>
    public ICollection<ModuleSecurityEntity> ModuleSecurities { get; set; } = new List<ModuleSecurityEntity>();
    
    /// <summary>
    /// Instances
    /// </summary>
    public ICollection<ModuleInstanceEntity> ModuleInstances { get; set; } = new List<ModuleInstanceEntity>();
}