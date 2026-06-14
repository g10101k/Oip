namespace Oip.Base.Data.Entities;

/// <summary>
/// Module security entity
/// </summary>
public class ModuleSecurityEntity
{
    /// <summary>
    /// Id
    /// </summary>
    public int ModuleSecurityId { get; set; }

    /// <summary>
    /// ModuleId
    /// </summary>
    public int ModuleId { get; set; }

    /// <summary>
    /// Right
    /// </summary>
    public string Right { get; set; } = null!;

    /// <summary>
    /// Role
    /// </summary>
    public string Role { get; set; } = null!;
    
    /// <summary>
    /// Module
    /// </summary>
    public ModuleEntity Module { get; set; } = null!;
}