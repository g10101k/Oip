namespace Oip.Base.Data.Entities;

/// <summary>
/// Module instance a user opens by default
/// </summary>
public class UserStartModuleEntity
{
    /// <summary>
    /// Id
    /// </summary>
    public int UserStartModuleId { get; set; }

    /// <summary>
    /// Stable user identifier taken from the subject claim (max 255 chars)
    /// </summary>
    public string UserSubject { get; set; } = null!;

    /// <summary>
    /// Module instance opened when no explicit route is requested
    /// </summary>
    public int ModuleInstanceId { get; set; }
}
