namespace Oip.Base.Data.Entities;

/// <summary>
/// Module instance security entity
/// </summary>
public class ModuleInstanceSecurityEntity
{
    /// <summary>
    /// Id
    /// </summary>
    public int ModuleInstanceSecurityId { get; set; }

    /// <summary>
    /// ModuleId
    /// </summary>
    public int ModuleInstanceId { get; set; }

    /// <summary>
    /// Right (max 255 chars)
    /// </summary>
    public string Right { get; set; } = null!;

    /// <summary>
    /// Role (max 255 chars)
    /// </summary>
    public string Role { get; set; } = null!;
    
    /// <summary>
    /// Module Instance
    /// </summary>
    public ModuleInstanceEntity ModuleInstance { get; set; } = null!;
}